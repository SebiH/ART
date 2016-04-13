#pragma once

#include "ITextureWriter.h"

namespace ImageProcessing
{
	class UnityDX11TextureWriter : public ITextureWriter
	{
	private:
		std::vector<unsigned char *> _texturePtrs;

	public:
		UnityDX11TextureWriter(std::vector<unsigned char *> &texturePtrs);
		~UnityDX11TextureWriter();

		virtual void writeTexture(const std::vector<ProcessingOutput> &processedImages) override;
	};

}
