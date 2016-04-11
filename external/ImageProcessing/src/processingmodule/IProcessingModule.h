#pragma once

#include <memory>
#include <vector>

namespace ImageProcessing
{
	class IProcessingModule
	{
	public:
		virtual ~IProcessingModule() {}
		virtual std::vector<std::unique_ptr<unsigned char[]>> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight) = 0;
	};

}
