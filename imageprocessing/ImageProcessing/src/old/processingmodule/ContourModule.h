#pragma once

#include "IProcessingModule.h"

namespace ImageProcessing
{
	class ContourModule : public IProcessingModule
	{
	public:
		ContourModule();
		~ContourModule();
		virtual std::vector<ProcessingOutput> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info) override;
		void FindContours(cv::Mat &src, cv::Mat &dst);
	};

}
