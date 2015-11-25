#pragma once

#define DllExport   __declspec( dllexport )

#include <opencv2/aruco.hpp>
#include <opencv2/opencv.hpp>


using namespace cv;

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


extern "C" DllExport unsigned char* DetectMarker(unsigned char *data, int width, int height, double *pose)
{
	Mat image = Mat(height, width, CV_8UC3, data);

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

		pose[0] = tvec[0];
		pose[1] = tvec[1];
		pose[2] = tvec[2];

		pose[3] = rvec[0];
		pose[4] = rvec[1];
		pose[5] = rvec[2];
	}

	// TODO return ... something?
	return NULL;
}
