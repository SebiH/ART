#pragma once

#include <memory>

namespace ImageProcessing
{
	template<class ExtraDataType> class FrameData
	{
		// ??? unnecessary because frame will be written into pipeline buffer?
		std::unique_ptr<unsigned char*> ImageDataRight;
		std::unique_ptr<unsigned char*> ImageDataLeft;
		std::unique_ptr<ExtraDataType*> MetaData;
	};
}
