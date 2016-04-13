#include "OpenCvTextureWriter.h"

#include <opencv2/highgui.hpp>
#include <math.h>

using namespace ImageProcessing;

OpenCvTextureWriter::OpenCvTextureWriter(std::string windowName)
	: _windowName(windowName)
{
	cv::namedWindow(_windowName);
}


OpenCvTextureWriter::~OpenCvTextureWriter()
{
	// nothing to destruct ..for now
}



void OpenCvTextureWriter::writeTexture(const std::vector<ProcessingOutput> &processedImages)
{
	int totalWidth = 0;
	int maxHeight = 0;

	for (int i = 0; i < processedImages.size(); i++)
	{
		auto currentSize = processedImages[i].img.size();
		maxHeight = std::max<int>(currentSize.height, maxHeight);
		totalWidth += currentSize.width;
	}

	if (totalWidth == 0 || maxHeight == 0)
	{
		return;
	}

	cv::Mat mergedMat(cv::Size(totalWidth, maxHeight), CV_8UC4);
	int currentOffset = 0;

	for (int i = 0; i < processedImages.size(); i++)
	{
		auto currentImage = processedImages[i].img;
		cv::Mat roi = cv::Mat(mergedMat, cv::Rect(cv::Point(currentOffset, 0), currentImage.size()));
		currentImage.copyTo(roi);
		currentOffset += currentImage.size().width;
	}

	imshow(_windowName, mergedMat);
}
