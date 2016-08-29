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


void OpenCvOutput::RegisterResult(const std::shared_ptr<const FrameData> &result)
{
	// write result immediately
	Write(result.get());

	Output::RegisterResult(result);
}

void OpenCvOutput::Write(const FrameData *result) noexcept
{
	// TODO: get cv type from framedata...?
	int cv_type;
	if (result->size.depth == 1)      { cv_type = CV_8UC1; }
	else if (result->size.depth == 3) { cv_type = CV_8UC3; }
	else if (result->size.depth == 4) { cv_type = CV_8UC4; }
	else { return; } // unknown depth

	cv::Mat merged(cv::Size(result->size.width * 2, result->size.height), cv_type);

	cv::Mat leftSrc(cv::Size(result->size.width, result->size.height), cv_type, result->buffer_left.get());
	cv::Mat left(merged, cv::Rect(cv::Point(0, 0), cv::Size(result->size.width, result->size.height)));
	leftSrc.copyTo(left);

	cv::Mat rightSrc(cv::Size(result->size.width, result->size.height), cv_type, result->buffer_right.get());
	cv::Mat right(merged, cv::Rect(cv::Point(result->size.width, 0), cv::Size(result->size.width, result->size.height)));
	rightSrc.copyTo(right);

	cv::imshow(windowname_, merged);
}
