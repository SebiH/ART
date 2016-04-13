#pragma once

#include <cstddef>
#include "IProcessingModule.h"

namespace ImageProcessing
{
	class RawImageModule : public IProcessingModule
	{
	private:
		std::size_t _imgMemorySize;
		int _imgWidth;
		int _imgHeight;

	public:
		RawImageModule(int imgWidth, int imgHeight, int imgDepth);
		~RawImageModule();
		virtual std::vector<ProcessingOutput> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight) override;
	};

}
