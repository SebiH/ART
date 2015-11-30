#pragma once

#define DllExport   __declspec( dllexport )

#include <opencv2/aruco.hpp>
#include <opencv2/opencv.hpp>
#include <math.h>


using namespace cv;

struct MarshalledImageData
{
	// Pointer to the byte array containing the image (*has* to be of length width x height x channels)
	unsigned char *rawData;

	// Width of the image
	int width;

	// Height of the image
	int height;

	//  Amount of channels in the image
	int channels;
};


struct MarshalledPose
{
	double translationX;
	double translationY;
	double translationZ;

	double rotationX;
	double rotationY;
	double rotationZ;
};


static bool readCameraParameters(std::string filename, Mat &camMatrix, Mat &distCoeffs) {
	FileStorage fs(filename, FileStorage::READ);
	if (!fs.isOpened())
		return false;
	fs["camera_matrix"] >> camMatrix;
	fs["distortion_coefficients"] >> distCoeffs;
	return true;
}

static void drawError(Mat& img, const String error)
{
	// TODO:
	// - line breaks?
	// - multiple errors should appear underneath each other, not in same position 
	putText(img, error, Point(0, 50), CV_FONT_HERSHEY_SIMPLEX, 0.5, Scalar(255, 0, 0), 1);
}


extern "C" DllExport void DetectMarker(MarshalledImageData *mImg, MarshalledPose *pose)
{
	// TODO: different channels based on MarshalledImageData!
	int type = CV_8UC3;

	Mat image = Mat(mImg->height, mImg->width, type, mImg->rawData);

	aruco::Dictionary dictionary = aruco::getPredefinedDictionary(aruco::PREDEFINED_DICTIONARY_NAME(aruco::DICT_5X5_250));

	// parameters from printed board
	int markersX = 5;
	int markersY = 3;
	int markerLength = 300;
	int markerSeparation = 75;

	auto board = aruco::GridBoard::create(5, 3, 300, 75, dictionary);
	float axisLength = 0.5f * ((float)min(markersX, markersY) * (markerLength + markerSeparation) + markerSeparation);

	bool showRejected = false;
	bool estimatePose = true;

	aruco::DetectorParameters detectorParams; // TODO?
	detectorParams.doCornerRefinement = true; // do corner refinement in markers

	Mat camMatrix, distCoeffs;
	auto cameraParamFilename = "Assets/CV/webcam.yml";
	bool readOk = readCameraParameters(cameraParamFilename, camMatrix, distCoeffs);
	if (!readOk)
	{
		drawError(image, "Could not find image calibration file!");
	}


	std::vector< int > ids;
	std::vector< std::vector< Point2f > > corners, rejected;
	Vec3d rvec, tvec;

	// detect markers and estimate pose
	int markersOfBoardDetected = 0;
	aruco::detectMarkers(image, dictionary, corners, ids, detectorParams, rejected);

	if (estimatePose && ids.size() > 0)
	{
		markersOfBoardDetected = aruco::estimatePoseBoard(corners, ids, board, camMatrix, distCoeffs, rvec, tvec);
	}

	// draw results
	if (ids.size() > 0)
	{
		aruco::drawDetectedMarkers(image, corners, ids);
	}

	if (showRejected && rejected.size() > 0)
	{
		aruco::drawDetectedMarkers(image, rejected, noArray(), Scalar(100, 0, 255));
	}

	if (markersOfBoardDetected > 0)
	{
		aruco::drawAxis(image, camMatrix, distCoeffs, rvec, tvec, axisLength);

		pose->translationX = tvec[0];
		pose->translationY = tvec[1];
		pose->translationZ = tvec[2];

		// to 3d rotation matrix
		Mat rot;
		Rodrigues(rvec, rot);

		// see: http://nghiaho.com/?page_id=846
		pose->rotationX = atan2(rot.at<double>(2, 1), rot.at<double>(2, 2));
		pose->rotationY = atan2(-(rot.at<double>(2, 0)), sqrt(pow(rot.at<double>(2, 1), 2) + pow(rot.at<double>(2, 2), 2)));
		pose->rotationZ = atan2(rot.at<double>(1, 0), rot.at<double>(0, 0));
	}
}
