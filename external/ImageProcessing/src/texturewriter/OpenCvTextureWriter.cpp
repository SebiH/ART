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



void OpenCvTextureWriter::WriteTexture(std::vector<cv::Mat> processedImages)
{
	auto maxHeight = 0;
	auto cumulativeWidth = 0;

	for (auto processedImage : processedImages)
	{
		maxHeight = std::max<int>(maxHeight, processedImage.size().height);
		cumulativeWidth += processedImage.size().width;
	}

	cv::Mat mergedMat(cv::Size(cumulativeWidth, maxHeight), CV_8UC4);

	auto currentOffset = 0;
	for (auto processedImage : processedImages)
	{
		cv::Mat roi = cv::Mat(mergedMat, cv::Rect(cv::Point(currentOffset, 0), processedImage.size()));
		processedImage.copyTo(roi);
		currentOffset += processedImage.size().width;
	}

	imshow(_windowName, mergedMat);
}
