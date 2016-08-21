#pragma once

#include <mutex>
#include <opencv2/core.hpp>

#include "IProcessingModule.h"

namespace ImageProcessing
{
	class RoiModule : public IProcessingModule
	{
	private:
		cv::Rect _roi;
		std::mutex _roiMutex;

	public:
		RoiModule(cv::Rect roi);
		~RoiModule();

		virtual std::vector<ProcessingOutput> processImage(unsigned char * rawDataLeft, unsigned char * rawDataRight, const ImageInfo &info) override;
		void setRegion(cv::Rect roi);
	};

}
