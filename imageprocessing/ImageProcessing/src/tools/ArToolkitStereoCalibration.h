#pragma once

#include <string>

#include "CalibrationSettings.h"

namespace ImageProcessing
{
	class ArToolkitStereoCalibration
	{
	private:
		std::string filename;
		CalibrationSettings settings;

	public:
		ArToolkitStereoCalibration(const CalibrationSettings &settings);
		~ArToolkitStereoCalibration();

		void InitCalibration(std::string filename);
		void SaveCalibrationImage();
	};
}
