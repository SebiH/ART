#pragma once

#include "ITextureWriter.h"
#include "../util/ProcessingOutput.h"

namespace ImageProcessing
{
	class UnityDX11TextureWriter : public ITextureWriter
	{
	private:
		void* _texturePtr;
		ProcessingOutput::Type _type;
		std::unique_ptr<unsigned char[]> _tempData;
		int prevOutputId;

	public:
		UnityDX11TextureWriter(void* texturePtr, ProcessingOutput::Type type);
		~UnityDX11TextureWriter();

		virtual void writeTexture(const std::vector<ProcessingOutput> &processedImages) override;
	};

}
