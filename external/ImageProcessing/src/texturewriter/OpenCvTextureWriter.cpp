#include "OpenCvTextureWriter.h"

#include <opencv2/highgui.hpp>
#include <math.h>

using namespace ImageProcessing;

OpenCvTextureWriter::OpenCvTextureWriter(std::string windowName, int expectedImageWidth, int expectedImageHeight, int expectedImageCount)
	: _windowName(windowName),
	  _expectedImageWidth(expectedImageWidth),
	  _expectedImageHeight(expectedImageHeight),
	  _expectedImageCount(expectedImageCount)
{
	cv::namedWindow(_windowName);
}


OpenCvTextureWriter::~OpenCvTextureWriter()
{
	// nothing to destruct ..for now
}



void OpenCvTextureWriter::writeTexture(const std::vector<ProcessingOutput> &processedImages)
{
	cv::Mat mergedMat(cv::Size(_expectedImageWidth * processedImages.size(), _expectedImageHeight), CV_8UC4);

	auto i = 0;

	//for (auto processedImage : processedImages)
	//{
	//	auto imgSize = cv::Size(_expectedImageWidth, _expectedImageHeight);
	//	cv::Mat roi = cv::Mat(mergedMat, cv::Rect(cv::Point(i * _expectedImageWidth, 0), imgSize));
	//	processedImage.img.copyTo(roi);
	//	i++;
	//}

	imshow(_windowName, mergedMat);
}
