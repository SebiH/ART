#pragma once

#include <vector>
#include "../util/ProcessingOutput.h"
#include "../util/ImageInfo.h"

namespace ImageProcessing
{
	class IProcessingModule
	{
	public:
		virtual ~IProcessingModule() {}
		virtual std::vector<ProcessingOutput> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info) = 0;
	};

}
