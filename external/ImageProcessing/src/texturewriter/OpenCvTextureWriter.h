#pragma once

#include "ITextureWriter.h"

#include <string>

namespace ImageProcessing
{
	class OpenCvTextureWriter : public ITextureWriter
	{
	private:
		std::string _windowName;
		int _expectedImageWidth;
		int _expectedImageHeight;
		int _expectedImageCount;

	public:
		OpenCvTextureWriter(std::string windowName, int expectedImageWidth, int expectedImageHeight, int expectedImageCount);
		~OpenCvTextureWriter();
		virtual void writeTexture(const std::vector<std::unique_ptr<unsigned char[]>> &processedImages) override;
	};

}
