#pragma once

#include "IProcessingModule.h"

namespace ImageProcessing
{
	class RawImageModule : public IProcessingModule
	{
	private:
		size_t _imgMemorySize;

	public:
		RawImageModule(int imgWidth, int imgHeight, int imgDepth);
		~RawImageModule();
		virtual std::vector<std::unique_ptr<unsigned char[]>> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight) override;
	};

}
