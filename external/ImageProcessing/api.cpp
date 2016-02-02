#pragma once

#define DllExport   __declspec( dllexport )

#include <opencv2/aruco.hpp>
#include <opencv2/aruco/charuco.hpp>
#include <opencv2/opencv.hpp>
#include <math.h>

#include <ovrvision_pro.h>

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

	aruco::Dictionary dictionary = aruco::getPredefinedDictionary(aruco::PREDEFINED_DICTIONARY_NAME(aruco::DICT_5X5_1000));

	// parameters from printed board
	int markersX = 8;
	int markersY = 5;
	int markerLength = 100;
	int squareLength = 200;

	auto board = aruco::CharucoBoard::create(markersX, markersY, squareLength, markerLength, dictionary);
	float axisLength = 0.5f * ((float)min(markersX, markersY) * (markerLength + squareLength) + squareLength);

	bool showRejected = false;
	bool estimatePose = true;
	bool refindStrategy = false;

	aruco::DetectorParameters detectorParams; // TODO?
	detectorParams.doCornerRefinement = true; // do corner refinement in markers

	Mat camMatrix, distCoeffs;
	auto cameraParamFilename = "Assets/CV/webcam.yml";
	bool readOk = readCameraParameters(cameraParamFilename, camMatrix, distCoeffs);
	if (!readOk)
	{
		drawError(image, "Could not find image calibration file!");
	}


	std::vector< int > markerIds, charucoIds;
	std::vector< std::vector< Point2f > > markerCorners, rejectedMarkers;
	std::vector< Point2f > charucoCorners;
	Mat rvec, tvec;

	// detect markers
	aruco::detectMarkers(image, dictionary, markerCorners, markerIds, detectorParams, rejectedMarkers);

	// refind strategy to detect more markers
	if (refindStrategy)
	{
		aruco::refineDetectedMarkers(image, board, markerCorners, markerIds, rejectedMarkers, camMatrix, distCoeffs);
	}

	// interpolate charuco corners
	int interpolatedCorners = 0;
	if (markerIds.size() > 0)
	{
		interpolatedCorners = aruco::interpolateCornersCharuco(markerCorners, markerIds, image, board, charucoCorners, charucoIds, camMatrix, distCoeffs);
	}

	// estimate charuco board pose
	bool validPose = false;
	if (camMatrix.total() != 0)
	{
		validPose = aruco::estimatePoseCharucoBoard(charucoCorners, charucoIds, board, camMatrix, distCoeffs, rvec, tvec);
	}


	// draw results
	if (markerIds.size() > 0)
	{
		aruco::drawDetectedMarkers(image, markerCorners);
	}

	if (showRejected && rejectedMarkers.size() > 0)
	{
		aruco::drawDetectedMarkers(image, rejectedMarkers, noArray(), Scalar(100, 0, 255));
	}

	if (interpolatedCorners > 0) {
		Scalar color;
		color = Scalar(0, 0, 255);
		aruco::drawDetectedCornersCharuco(image, charucoCorners, charucoIds, color);
	}

	if (validPose)
	{
		aruco::drawAxis(image, camMatrix, distCoeffs, rvec, tvec, axisLength);

		//pose->translationX = tvec[0];
		//pose->translationY = tvec[1];
		//pose->translationZ = tvec[2];

		// to 3d rotation matrix
		//Mat rot;
		//Rodrigues(rvec, rot);

		//// see: http://nghiaho.com/?page_id=846
		//pose->rotationX = atan2(rot.at<double>(2, 1), rot.at<double>(2, 2));
		//pose->rotationY = atan2(-(rot.at<double>(2, 0)), sqrt(pow(rot.at<double>(2, 1), 2) + pow(rot.at<double>(2, 2), 2)));
		//pose->rotationZ = atan2(rot.at<double>(1, 0), rot.at<double>(0, 0));
	}
}



extern "C" DllExport void RunOvrTest()
{
	int locationID = 0;
	OVR::Camprop cameraMode = OVR::OV_CAMVR_FULL;

	// Create Ovrvision object
	OVR::OvrvisionPro g_pOvrvision;
	if (g_pOvrvision.Open(locationID, cameraMode) == 0) {	//Open 960x950@60 default
		printf("Ovrvision Pro Open Error!\nPlease check whether OvrvisionPro is connected.");
	}

	// camera settings
	g_pOvrvision.SetCameraExposure(12960);
	g_pOvrvision.SetCameraGain(47);

	g_pOvrvision.SetCameraSyncMode(false);

	auto g_camWidth = g_pOvrvision.GetCamWidth();
	auto g_camHeight = g_pOvrvision.GetCamHeight();

	auto g_processMode = OVR::Camqt::OV_CAMQT_DMS;

	while (g_pOvrvision.isOpen())
	{
		//Full Draw
		g_pOvrvision.PreStoreCamData(g_processMode);
		unsigned char* p = g_pOvrvision.GetCamImageBGRA(OVR::OV_CAMEYE_LEFT);
		unsigned char* p2 = g_pOvrvision.GetCamImageBGRA(OVR::OV_CAMEYE_RIGHT);

		cv::Mat leftImg(g_camHeight, g_camWidth, CV_8UC4);
		cv::Mat rightImg(g_camHeight, g_camWidth, CV_8UC4);
		leftImg.data = p;
		rightImg.data = p2;

		cv::Mat compositeImg(g_camHeight, g_camWidth * 2, CV_8UC4);
		cv::Mat leftROI(compositeImg, cv::Range(0, g_camHeight), cv::Range(0, g_camWidth));
		leftImg.copyTo(leftROI);
		cv::Mat rightROI(compositeImg, cv::Range(0, g_camHeight), cv::Range(g_camWidth, g_camWidth * 2));
		rightImg.copyTo(rightROI);

		cv::imshow("x", compositeImg);
		cv::waitKey(30);
	}
}
