#pragma once

namespace ImageProcessing
{
	class ITextureWriter
	{
	public:
		virtual ~ITextureWriter() {}
		virtual void WriteTexture(unsigned char *rawData) = 0;
	};
}
