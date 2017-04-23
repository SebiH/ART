#pragma once

#include <string>
#include <opencv2/opencv.hpp>
#include "cameras/CameraSourceInterface.h"

namespace ImageProcessing
{
	class StandardCalibrator
	{
	public:
		int corners_num_x = 7;
		int corners_num_y = 5;
		double pattern_width = 24.0; // in mm?
		int calib_image_count = 10;
		double screen_size_margin = 0.1;

	private:
		cv::Mat camera_matrix_;
		cv::Mat dist_coeffs_;

		cv::Mat camera_matrix_l_;
		cv::Mat dist_coeffs_l_;
		cv::Mat camera_matrix_r_;
		cv::Mat dist_coeffs_r_;


	public:
		StandardCalibrator();
		void Calibrate(const std::shared_ptr<CameraSourceInterface> &camera, const std::string &filename);

	private:
		void SingleCameraCalibration(const std::string &filename, const std::vector<std::vector<cv::Point2f>> &image_points, const cv::Size &image_size);
		void StereoCameraCalibration(const std::string &filename, const std::vector<std::vector<cv::Point2f>> &image_points_l, const std::vector<std::vector<cv::Point2f>> &image_points_r, const cv::Size &image_size);
	};
}
