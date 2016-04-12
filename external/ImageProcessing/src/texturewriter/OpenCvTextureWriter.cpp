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



void OpenCvTextureWriter::writeTexture(const std::vector<std::unique_ptr<unsigned char[]>> &processedImages)
{
	cv::Mat mergedMat(cv::Size(_expectedImageWidth * processedImages.size(), _expectedImageHeight), CV_8UC4);

	for (int i = 0; i < processedImages.size(); i++)
	{
		auto imgSize = cv::Size(_expectedImageWidth, _expectedImageHeight);
		auto imgRawData = processedImages.at(i).get();
		cv::Mat imgMat(imgSize.height, imgSize.width, CV_8UC4, imgRawData);
		cv::Mat roi = cv::Mat(mergedMat, cv::Rect(cv::Point(i * _expectedImageWidth, 0), imgSize));
		imgMat.copyTo(roi);
	}

	imshow(_windowName, mergedMat);
}
