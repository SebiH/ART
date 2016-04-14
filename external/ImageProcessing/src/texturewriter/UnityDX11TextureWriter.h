#pragma once

#include "ITextureWriter.h"
#include "../ProcessingOutput.h"

namespace ImageProcessing
{
	class UnityDX11TextureWriter : public ITextureWriter
	{
	private:
		unsigned char * _texturePtr;
		ProcessingOutput::Type _type;

	public:
		UnityDX11TextureWriter(unsigned char *texturePtr, ProcessingOutput::Type type);
		~UnityDX11TextureWriter();

		virtual void writeTexture(const std::vector<ProcessingOutput> &processedImages) override;
	};

}
