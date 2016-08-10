#pragma once

#include <memory>
#include "../framesource/IFrameSource.h"

namespace ImageProcessing
{
	struct CalibrationSettings
	{
		int CornersX;
		int CornersY;
		double PatternWidth;
		double ScreenSizeMargin;

		/// Amount of images needed until calibration is complete
		int CalibrationImageCount;

		std::shared_ptr<IFrameSource> FrameSource;

		CalibrationSettings(std::shared_ptr<IFrameSource> &frameSource) :
			CornersX(7),
			CornersY(5),
			PatternWidth(30.0),
			ScreenSizeMargin(0.1),
			CalibrationImageCount(10),
			FrameSource(frameSource)
		{
		}
	};
}
