#pragma once

#include <vector>
#include "../util/ProcessingOutput.h"

namespace ImageProcessing
{
	class ITextureWriter
	{
	public:
		virtual ~ITextureWriter() {}
		virtual void writeTexture(const std::vector<ProcessingOutput> &processedImages) = 0;
	};
}
