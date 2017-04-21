#include "ArToolkitCalibrator.h"

#include <opencv2/highgui.hpp>

#include "frames/FrameSize.h"

using namespace ImageProcessing;

ArToolkitCalibrator::ArToolkitCalibrator()
	: cornerSet(std::unique_ptr<cv::Point2d[]>(new cv::Point2d[corners_num_x * corners_num_y * calib_image_count], std::default_delete<cv::Point2d[]>()))
{
}

void ArToolkitCalibrator::Calibrate(const std::shared_ptr<CameraSourceInterface> &camera, const std::string &filename)
{
	if (!camera->IsOpen())
	{
		camera->Open();
	}

	auto frame_size = FrameSize(camera->GetFrameWidth(), camera->GetFrameHeight(), camera->GetFrameChannels());
	auto mat_size = cv::Size(frame_size.width, frame_size.height);
	auto buffer_left = std::unique_ptr<unsigned char[]>(new unsigned char[frame_size.BufferSize()], std::default_delete<unsigned char[]>());
	auto buffer_right = std::unique_ptr<unsigned char[]>(new unsigned char[frame_size.BufferSize()], std::default_delete<unsigned char[]>());

	cv::Size pattern_size(corners_num_x, corners_num_y);

	while (true)
	{
		camera->PrepareNextFrame();
		camera->GrabFrame(buffer_left.get(), buffer_right.get());

		cv::Mat img(mat_size, frame_size.CvType(), eye == 0 ? buffer_left.get() : buffer_right.get());
		cv::Mat img_gray(mat_size, CV_8UC1);
		cv::cvtColor(img, img_gray, frame_size.CvToGray());

		cv::Mat corners;
		auto found_corners = cv::findChessboardCorners(img_gray, pattern_size, corners, CV_CALIB_CB_ADAPTIVE_THRESH | CV_CALIB_CB_FILTER_QUADS);
		cv::drawChessboardCorners(img, pattern_size, corners, found_corners);

		cv::imshow("", img);
		int pressedKey = cv::waitKey(10);

		if (pressedKey == ' ')
		{
			// perform calibration
			break;
		}
	}

	camera->Close();
}
