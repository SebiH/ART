#pragma once

#include <AR/ar.h>

#include "IProcessingModule.h"

namespace ImageProcessing
{
	class ARToolkitModule : public IProcessingModule
	{
	private:
		bool isInitialized = false;

		int patt_id;
		ARParam cparam;
		ARParamLT *cparamLT_p;
		ARHandle *arHandle;
		ARPattHandle *arPattHandle;
		AR3DHandle *ar3DHandle;

	public:
		ARToolkitModule();
		~ARToolkitModule();
		virtual std::vector<ProcessingOutput> processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info) override;

	private:
		void initialize(int sizeX, int sizeY);
		void cleanup();
	};

}
