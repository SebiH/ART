#pragma once

#include <mutex>
#include <opencv2/core.hpp>

#include "IProcessingModule.h"

namespace ImageProcessing
{
	class RoiModule : public IProcessingModule
	{
	private:
		int _maxWidth;
		int _maxHeight;
		cv::Rect _roi;
		std::mutex _roiMutex;

	public:
		RoiModule(int camWidth, int camHeight);
		~RoiModule();

		virtual std::vector<ProcessingOutput> processImage(unsigned char * rawDataLeft, unsigned char * rawDataRight) override;
		void setRegion(cv::Rect roi);
	};

}
