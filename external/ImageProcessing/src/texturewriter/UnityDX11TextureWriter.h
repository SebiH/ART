#pragma once

#include "ITextureWriter.h"

namespace ImageProcessing
{
	class UnityDX11TextureWriter : public ITextureWriter
	{
	private:
		unsigned char *_texturePtr;

	public:
		UnityDX11TextureWriter(unsigned char *texturePtr);
		~UnityDX11TextureWriter();

		virtual void writeTexture(const std::vector<std::unique_ptr<unsigned char[]>> &processedImages) override;
	};

}
