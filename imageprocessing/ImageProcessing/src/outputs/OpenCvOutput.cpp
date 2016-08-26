#include "outputs/OpenCvOutput.h"

#include <opencv2/core.hpp>
#include <opencv2/highgui.hpp>

using namespace ImageProcessing;

OpenCvOutput::OpenCvOutput(std::string windowname)
	: windowname_(windowname)
{
	cv::namedWindow(windowname);
}

OpenCvOutput::~OpenCvOutput()
{
	cv::destroyWindow(windowname_);
}


void OpenCvOutput::RegisterResult(const FrameData &result)
{
	Output::RegisterResult(result);

	// write result immediately
	Write(result);
}

void OpenCvOutput::Write(const FrameData &result) noexcept
{
	cv::Mat merged(cv::Size(result.size.width * 2, result.size.height), CV_8UC4);

	cv::Mat left(merged, cv::Rect(cv::Point(0, 0), cv::Size(result.size.width, result.size.height)));
	cv::Mat right(merged, cv::Rect(cv::Point(result.size.width, 0), cv::Size(result.size.width, result.size.height)));

	imshow(windowname_, merged);
}
