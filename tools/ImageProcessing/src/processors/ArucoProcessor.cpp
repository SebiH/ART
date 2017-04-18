#include "ArucoProcessor.h"

#include <opencv2/opencv.hpp>
#include <aruco/cvdrawingutils.h>

#include "cameras/ActiveCamera.h"
#include "cameras/CameraSourceInterface.h"
#include "frames/JsonFrameData.h"

using namespace ImageProcessing;
using json = nlohmann::json;

namespace Params
{
	static const auto use_tracker = "use_tracker";
	static const auto tracker_error = "tracker_error_ratio";
	static const auto marker_size = "marker_size_m";
}

ArucoProcessor::ArucoProcessor(const json &marker_config)
	: initialized_size_(-1, -1, -1)
{
	SetProperties(marker_config);

	// detector settings
	// TODO: deprecated! use setParams, allow settings via Get/SetProperties
	detector_.setDictionary(aruco::Dictionary::ARTOOLKITPLUSBCH);
	detector_.setCornerRefinementMethod(aruco::MarkerDetector::LINES);
	detector_.setThresholdMethod(aruco::MarkerDetector::ADPT_THRES);
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

	if (use_tracker_)
	{
		detected_markers = detector_.detect(img_gray);
	}
	else
	{
		// TODO: crashes with release version of aruco, works in debug?
		detector_.detect(img_gray, detected_markers, camera_params_.CameraMatrix, cv::Mat(), marker_size_m_, true);
	}

	json processed_markers{
		{ "markers_left", json::array() },
		{ "markers_right", json::array() }
	};

	for (auto &marker : detected_markers)
	{
		if (!marker.isValid())
		{
			continue;
		}

		cv::Mat rvec, tvec;

		if (use_tracker_)
		{
			if (pose_trackers_[marker.id].estimatePose(marker, camera_params_, marker_size_m_, pt_min_error_ratio_))
			{
				//rvec = pose_trackers_[marker.id].getRvec();
				//tvec = pose_trackers_[marker.id].getTvec();
				rvec = marker.Rvec;
				tvec = marker.Tvec;
			}
			else
			{
				continue;
			}
		}
		else
		{
			rvec = marker.Rvec;
			tvec = marker.Tvec;
		}


		// Conversion to unity coordinate system
		rvec.at<float>(0, 0) = -rvec.at<float>(0, 0);
		rvec.at<float>(2, 0) = -rvec.at<float>(2, 0);
		cv::Mat rotation(3, 3, CV_32FC1);
		cv::Rodrigues(rvec, rotation);

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
		Quaternion::RotMatToQuaternion(&quat, rotation_matrix);

		// ??? (Taken from OvrVision Pro)
		quat.w = -quat.w;
		Quaternion adjustment{ 1.0f, 0.0f, 0.0f, 0.0f };
		quat = Quaternion::MultiplyQuaternion(&quat, &adjustment);

		// Draw markers on image
		marker.draw(img_left, cv::Scalar(0, 0, 255, 255));

		// Save JSON info about marker
		processed_markers["markers_left"].push_back(json{
			{ "id", marker.id },
			{ "position", {
				{ "x", tvec.at<float>(0, 0) },
				{ "y", tvec.at<float>(1, 0) },
				{ "z", tvec.at<float>(2, 0) }
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
	return json{
		{ Params::marker_size, marker_size_m_ },
		{ Params::use_tracker, use_tracker_ },
		{ Params::tracker_error, pt_min_error_ratio_ }
	};
}

void ArucoProcessor::SetProperties(const json &config)
{
	if (config.count(Params::marker_size))
	{
		marker_size_m_ = config[Params::marker_size].get<float>();
	}

	if (config.count(Params::use_tracker))
	{
		use_tracker_ = config[Params::use_tracker].get<bool>();
	}

	if (config.count(Params::tracker_error))
	{
		pt_min_error_ratio_ = config[Params::tracker_error].get<float>();
	}

	// reset all trackers to remove any existing data
	pose_trackers_ = std::map<int, aruco::MarkerPoseTracker>();

	// TODO detector_->getParams(); and move methods from constructor to here
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
