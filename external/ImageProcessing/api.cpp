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


extern "C" DllExport int* DetectMarker(int *data, int width, int height)
{
	Mat test = Mat(width, height, CV_8UC3, data);

	return data;

	VideoCapture cap(0); //capture the video from web cam
	bool consoleActive = false;
	std::ostringstream consoleCommand;

	if (!cap.isOpened())  // if not success, exit program
	{
		std::cout << "Cannot open the web cam" << std::endl;
		return NULL;
	}

	bool useBlurring = false;

	aruco::Dictionary dictionary =
		aruco::getPredefinedDictionary(aruco::PREDEFINED_DICTIONARY_NAME(aruco::DICT_5X5_50));

	//Mat marker;
	//aruco::drawMarker(dictionary, 1, 640, marker, 1);
	//imshow("x", marker);

	bool showRejected = false;
	bool estimatePose = true;
	float markerLength = 1.0f;
	aruco::DetectorParameters detectorParams; // TODO?
	detectorParams.doCornerRefinement = true; // do corner refinement in markers

	Mat camMatrix, distCoeffs;
	bool readOk = readCameraParameters("webcam.yml", camMatrix, distCoeffs);
	if (!readOk) {
		std::cerr << "Invalid camera file" << std::endl;
		return 0;
	}
	estimatePose = true;



	double totalTime = 0;
	int totalIterations = 0;
	int waitTime = 30;

	while (true)
	{
		Mat image, imageCopy;

		// read a new frame from video
		bool bSuccess = cap.read(image);

		// break on error
		if (!bSuccess)
		{
			std::cout << "Cannot read a frame from video stream" << std::endl;
			break;
		}

		if (useBlurring)
		{
			GaussianBlur(image, image, cv::Size(3, 3), 5, 5);
			circle(image, cv::Point(50, 50), 20, cv::Scalar(255, 0, 0));
		}
		else
		{
			circle(image, cv::Point(50, 50), 20, cv::Scalar(255, 0, 0));
		}

		double tick = (double)getTickCount();

		std::vector< int > ids;
		std::vector< std::vector< Point2f > > corners, rejected;
		std::vector< Vec3d > rvecs, tvecs;

		// detect markers and estimate pose
		aruco::detectMarkers(image, dictionary, corners, ids, detectorParams, rejected);
		if (estimatePose && ids.size() > 0)
		{
			aruco::estimatePoseSingleMarkers(corners, markerLength, camMatrix, distCoeffs, rvecs, tvecs);
		}

		double currentTime = ((double)getTickCount() - tick) / getTickFrequency();
		totalTime += currentTime;
		totalIterations++;
		if (totalIterations % 30 == 0)
		{
			std::cout << "Detection Time = " << currentTime * 1000 << " ms "
				<< "(Mean = " << 1000 * totalTime / double(totalIterations) << " ms)" << std::endl;
		}

		// draw results
		image.copyTo(imageCopy);
		if (ids.size() > 0)
		{
			aruco::drawDetectedMarkers(imageCopy, corners, ids);

			if (estimatePose)
			{
				for (unsigned int i = 0; i < ids.size(); i++)
				{
					aruco::drawAxis(imageCopy, camMatrix, distCoeffs, rvecs[i], tvecs[i], markerLength * 0.5f);
				}
			}
		}

		if (showRejected && rejected.size() > 0)
			aruco::drawDetectedMarkers(imageCopy, rejected, noArray(), Scalar(100, 0, 255));

		imshow("out", imageCopy);
		char key = (char)waitKey(waitTime);
		if (key == 27) break;
		if (key == 'a') useBlurring = !useBlurring;


	}

	// TODO return image ... byte array?
	return NULL;
}
