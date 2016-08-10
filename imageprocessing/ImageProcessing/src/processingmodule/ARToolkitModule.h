#pragma once

#include <AR/ar.h>

#include "IProcessingModule.h"

namespace ImageProcessing
{
	class ARToolkitModule : public IProcessingModule
	{
	private:
		ARHandle *arHandle;
		ARPattHandle *arPattHandle;

	public:
		ARToolkitModule();
		~ARToolkitModule();
		virtual std::vector<ProcessingOutput> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info) override;
	};

}
