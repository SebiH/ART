#pragma once

#include <vector>
#include "../ProcessingOutput.h"

namespace ImageProcessing
{
	class IProcessingModule
	{
	public:
		virtual ~IProcessingModule() {}
		virtual std::vector<ProcessingOutput> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight) = 0;
	};

}
