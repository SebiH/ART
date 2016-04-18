#pragma once

#include "IProcessingModule.h"

namespace ImageProcessing
{
	class RawImageModule : public IProcessingModule
	{
	public:
		RawImageModule();
		~RawImageModule();
		virtual std::vector<ProcessingOutput> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info) override;
	};

}
