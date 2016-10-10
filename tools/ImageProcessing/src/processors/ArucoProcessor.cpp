#include "ArucoProcessor.h"

#include <opencv2/opencv.hpp>
#include <aruco/cvdrawingutils.h>

#include "cameras/ActiveCamera.h"
#include "cameras/CameraSourceInterface.h"
#include "frames/JsonFrameData.h"

using namespace ImageProcessing;
using json = nlohmann::json;

ArucoProcessor::ArucoProcessor(const json &marker_config)
	: initialized_size_(-1, -1, -1)
{
	if (marker_config.count("marker_size_m"))
	{
		marker_size_m_ = marker_config["marker_size_m"].get<float>();
	}

	// detector settings
	// TODO: deprecated! use setParams, allow settings via Get/SetProperties
	detector_.setCornerRefinementMethod(aruco::MarkerDetector::LINES);
	detector_.setThresholdMethod(aruco::MarkerDetector::FIXED_THRES);
	detector_.setThresholdParams(threshold_, 0.0);
}

ArucoProcessor::~ArucoProcessor()
{

}

std::shared_ptr<const FrameData> ArucoProcessor::Process(const std::shared_ptr<const FrameData>& frame)
{
	if (initialized_size_ != frame->size)
	{
		Init(frame->size, ActiveCamera::Instance()->GetSource()->GetFocalLength());
	}

	cv::Size framesize(frame->size.width, frame->size.height);
	auto img_gray = cv::Mat(framesize, CV_MAKETYPE(CV_8U, 1));
	// TODO: currently only processes markers on left camera images!
	auto img_left = cv::Mat(framesize, CV_MAKETYPE(CV_8U, frame->size.depth), frame->buffer_left.get());
	// TODO: only works on 4channel image due to hardcoded BGRA2GRAY
	cv::cvtColor(img_left, img_gray, CV_BGRA2GRAY);

	std::vector<aruco::Marker> detected_markers;
	// TODO: crashes with release version of aruco, works in debug?
	detector_.detect(img_gray, detected_markers, camera_params_.CameraMatrix, cv::Mat(), marker_size_m_, true);

	json processed_markers{
		{ "markers_left", json::array() },
		{ "markers_right", json::array() }
	};

	for (auto &marker : detected_markers)
	{
		// Conversion to unity coordinate system
		marker.Rvec.at<float>(0, 0) = -marker.Rvec.at<float>(0, 0);
		marker.Rvec.at<float>(2, 0) = -marker.Rvec.at<float>(2, 0);
		cv::Mat rotation(3, 3, CV_32FC1);
		cv::Rodrigues(marker.Rvec, rotation);

		float rotation_matrix[16];
		rotation_matrix[0] = rotation.at<float>(0);
		rotation_matrix[1] = rotation.at<float>(1);
		rotation_matrix[2] = rotation.at<float>(2);
		rotation_matrix[3] = 0.0f;
		rotation_matrix[4] = rotation.at<float>(3);
		rotation_matrix[5] = rotation.at<float>(4);
		rotation_matrix[6] = rotation.at<float>(5);
		rotation_matrix[7] = 0.0f;
		rotation_matrix[8] = rotation.at<float>(6);
		rotation_matrix[9] = rotation.at<float>(7);
		rotation_matrix[10] = rotation.at<float>(8);
		rotation_matrix[11] = 0.0f;
		rotation_matrix[12] = 0.0f;
		rotation_matrix[13] = 0.0f;
		rotation_matrix[14] = 0.0f;
		rotation_matrix[15] = 1.0f;

		Quaternion quat;
		RotMatToQuaternion(&quat, rotation_matrix);

		// ??? (Taken from OvrVision Pro)
		quat.w = -quat.w;
		Quaternion adjustment{ 1.0f, 0.0f, 0.0f, 0.0f };
		quat = MultiplyQuaternion(&quat, &adjustment);

		// Draw markers on image
		cv::circle(img_left, marker.getCenter(), 5, cv::Scalar(0, 0, 255, 255), 1);

		// Save JSON info about marker
		processed_markers["markers_left"].push_back(json{
			{ "id", marker.id },
			{ "position", {
				{ "x", marker.Tvec.at<float>(0, 0) },
				{ "y", marker.Tvec.at<float>(1, 0) },
				{ "z", marker.Tvec.at<float>(2, 0) }
			}},
			{ "rotation", {
				{ "x", quat.x },
				{ "y", quat.y },
				{ "z", quat.z },
				{ "w", quat.w },
			}}
		});
	}


	if (detected_markers.size() > 0)
	{
		return std::make_shared<const JsonFrameData>(frame.get(), processed_markers);
	}
	else
	{
		return frame;
	}
}



json ArucoProcessor::GetProperties()
{
	// TODO detector_->getParams();
	return json();
}

void ArucoProcessor::SetProperties(const json &config)
{
	// TODO detector_->setParmas();
}



void ArucoProcessor::Init(const FrameSize &size, const float focalpoint)
{
	// below adapted from OVRVision, so it should be correct for OVRvision pro cameras
	float focalpoint_scale = 1.0f;

	if (size.width > 1280)
	{
		focalpoint_scale = 2.0f;
	}
	else if (size.width <= 640) {
		if (size.width <= 320)
		{
			focalpoint_scale = 0.25f;
		}
		else
		{
			focalpoint_scale = 0.5f;
		}
	}

	cv::Mat cameramat(3, 3, CV_32FC1);
	cameramat.at<float>(0) = focalpoint * focalpoint_scale;	//f
	cameramat.at<float>(1) = 0.0f;
	cameramat.at<float>(2) = ((float)size.width) / 2.0f;
	cameramat.at<float>(3) = 0.0f;
	cameramat.at<float>(4) = focalpoint * focalpoint_scale;
	cameramat.at<float>(5) = ((float)size.height) / 2.0f;
	cameramat.at<float>(6) = 0.0f;
	cameramat.at<float>(7) = 0.0f;
	cameramat.at<float>(8) = 1.0f;

	cv::Mat distorsionCoeff(4, 1, CV_32FC1);
	camera_params_.setParams(cameramat, distorsionCoeff, cv::Size(size.width, size.height));

	initialized_size_ = size;
}

// Adapted from OVRVision
void ArucoProcessor::RotMatToQuaternion(Quaternion * outQuat, const float * inMat)
{
	float s;
	float tr = inMat[0] + inMat[5] + inMat[10] + 1.0f;
	if (tr >= 1.0f) {
		s = 0.5f / sqrtf(tr);
		outQuat->w = 0.25f / s;
		outQuat->x = (inMat[6] - inMat[9]) * s;
		outQuat->y = (inMat[8] - inMat[2]) * s;
		outQuat->z = (inMat[1] - inMat[4]) * s;
		return;
	}
	else {
		float max;
		max = inMat[5] > inMat[10] ? inMat[5] : inMat[10];

		if (max < inMat[0]) {
			s = sqrtf(inMat[0] - inMat[5] - inMat[10] + 1.0f);
			float x = s * 0.5f;
			s = 0.5f / s;
			outQuat->x = x;
			outQuat->y = (inMat[1] + inMat[4]) * s;
			outQuat->z = (inMat[8] + inMat[2]) * s;
			outQuat->w = (inMat[6] - inMat[9]) * s;
			return;
		}
		else if (max == inMat[5]) {
			s = sqrtf(-inMat[0] + inMat[5] - inMat[10] + 1.0f);
			float y = s * 0.5f;
			s = 0.5f / s;
			outQuat->x = (inMat[1] + inMat[4]) * s;
			outQuat->y = y;
			outQuat->z = (inMat[6] + inMat[9]) * s;
			outQuat->w = (inMat[8] - inMat[2]) * s;
			return;
		}
		else {
			s = sqrtf(-inMat[0] - inMat[5] + inMat[10] + 1.0f);
			float z = s * 0.5f;
			s = 0.5f / s;
			outQuat->x = (inMat[8] + inMat[2]) * s;
			outQuat->y = (inMat[6] + inMat[9]) * s;
			outQuat->z = z;
			outQuat->w = (inMat[1] - inMat[4]) * s;
			return;
		}
	}

}

ArucoProcessor::Quaternion ImageProcessing::ArucoProcessor::MultiplyQuaternion(Quaternion * a, Quaternion * b)
{
	Quaternion ans;
	ans.w = a->w * b->w - a->x * b->x - a->y * b->y - a->z * b->z;
	ans.x = a->w * b->x + b->w * a->x + a->y * b->z - b->y * a->z;
	ans.y = a->w * b->y + b->w * a->y + a->z * b->x - b->z * a->x;
	ans.z = a->w * b->z + b->w * a->z + a->x * b->y - b->x * a->y;
	return ans;
}
