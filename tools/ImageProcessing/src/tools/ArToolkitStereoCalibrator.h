#pragma once

#include <memory>
#include <string>
#include <vector>
#include <opencv2/opencv.hpp>
#include <AR/ar.h>

#include "cameras/CameraSourceInterface.h"


namespace ImageProcessing
{
	class ArToolkitStereoCalibrator
	{
	public:
		int corners_num_x = 7;
		int corners_num_y = 5;
		double pattern_width = 24.0; // in mm?
		int calib_image_count = 10;
		double screen_size_margin = 0.1;
		std::string calibration_file_left;
		std::string calibration_file_right;

	public:
		ArToolkitStereoCalibrator();
		void Calibrate(const std::shared_ptr<CameraSourceInterface> &camera, const std::string &filename);

	private:
		void CopyImage(ARUint8 *p1, ARUint8 *p2, int size, int pixFormat);
		void SaveParam(ARdouble transL2R[3][4], const std::string &filename);

	};
}
