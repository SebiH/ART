#pragma once

#include <opencv2/core/mat.hpp>

namespace ImageProcessing
{
	class ITextureWriter
	{
	public:
		virtual ~ITextureWriter() {}
		virtual void WriteTexture(std::vector<cv::Mat> processedImages) = 0;
	};
}
