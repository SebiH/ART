#pragma once

#include "ITextureWriter.h"

#include <string>

namespace ImageProcessing
{
	class OpenCvTextureWriter : public ITextureWriter
	{
	private:
		std::string _windowName;

	public:
		OpenCvTextureWriter(std::string windowName);
		~OpenCvTextureWriter();
		virtual void WriteTexture(std::vector<cv::Mat> processedImages) override;
	};

}
