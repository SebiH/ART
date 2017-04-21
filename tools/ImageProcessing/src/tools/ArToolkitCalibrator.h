#pragma once

#include <memory>
#include <string>
#include <opencv2/opencv.hpp>

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

		int eye = 0; // 0 -> left, 1 -> right

	private:
		std::unique_ptr<cv::Point2d[]> cornerSet;

	public:
		ArToolkitCalibrator();
		void Calibrate(const std::shared_ptr<CameraSourceInterface> &camera, const std::string &filename);
	};
}
