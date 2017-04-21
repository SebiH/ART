#pragma once

#include <memory>
#include <string>
#include <vector>
#include <opencv2/opencv.hpp>
#include <AR/ar.h>

#include "cameras/CameraSourceInterface.h"


namespace ImageProcessing
{
	class ArToolkitCalibrator
	{
	public:
		int corners_num_x = 7;
		int corners_num_y = 5;
		double pattern_width = 24.0; // in mm?
		int calib_image_count = 10;
		double screen_size_margin = 0.1;

	public:
		ArToolkitCalibrator();
		void Calibrate(const std::shared_ptr<CameraSourceInterface> &camera, const std::string &filename);

	private:
		double computeReprojectionErrors(const std::vector<std::vector<cv::Point3f>> &object_points, const std::vector<std::vector<cv::Point2f>> &image_points, const std::vector<cv::Mat> &rvecs, const std::vector<cv::Mat> &tvecs, const cv::Mat &camera_matrix, const cv::Mat &dist_coeffs, std::vector<float> &per_view_errors);
		double SingleCameraCalibration(const std::string &filename, const std::vector<std::vector<cv::Point2f>> &image_points, const cv::Size &image_size);
		void ArToolkitCalibration(const std::string &filename, int xsize, int ysize, const std::vector<cv::Point2f> &cornerSet);
		void ConvParam(float intr[3][4], float dist[4], int xsize, int ysize, ARParam *param);
		void SaveParam(ARParam *param, const std::string &filename);
		ARdouble GetSizeFactor(ARdouble dist_factor[], int xsize, int ysize, int dist_function_version);
	};
}
