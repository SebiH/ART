#pragma once

#include <string>

#include "CalibrationSettings.h"

namespace ImageProcessing
{
	class ArToolkitCameraCalibration
	{
	private:
		std::string filename;
		CalibrationSettings settings;

	public:
		ArToolkitCameraCalibration(const CalibrationSettings &settings);
		~ArToolkitCameraCalibration();

		void InitCalibration(std::string filename);
		void SaveCalibrationImage();
	};
}
