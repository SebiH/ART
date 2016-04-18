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
	int matType = -1;

	for (auto const &processedImg : processedImages)
	{
		matType = processedImg.img.type();
		auto currentSize = processedImg.img.size();
		maxHeight = std::max<int>(currentSize.height, maxHeight);
		totalWidth += currentSize.width;
	}

	if (totalWidth == 0 || maxHeight == 0 || matType == -1)
	{
		return;
	}

	cv::Mat mergedMat(cv::Size(totalWidth, maxHeight), matType);
	int currentOffset = 0;

	for (auto const &processedImg : processedImages)
	{
		auto currentImage = processedImg.img;
		cv::Mat roi = cv::Mat(mergedMat, cv::Rect(cv::Point(currentOffset, 0), currentImage.size()));
		currentImage.copyTo(roi);
		currentOffset += currentImage.size().width;
	}

	imshow(_windowName, mergedMat);
}
