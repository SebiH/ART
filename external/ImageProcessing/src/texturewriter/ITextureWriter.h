#pragma once

#include <memory>
#include <vector>

namespace ImageProcessing
{
	class ITextureWriter
	{
	public:
		virtual ~ITextureWriter() {}
		virtual void writeTexture(const std::vector<std::unique_ptr<unsigned char[]>> &processedImages) = 0;
	};
}
