#include "ContourModule.h"

#include <opencv2/imgproc.hpp>

using namespace ImageProcessing;

ContourModule::ContourModule()
{

}

ContourModule::~ContourModule()
{

}

void ContourModule::FindContours(cv::Mat &src, cv::Mat &dst)
{
	cv::Mat preprocessedImg, edges;
	cv::cvtColor(src, preprocessedImg, CV_BGRA2GRAY);
	//cv::GaussianBlur(preprocessedImg, preprocessedImg, cv::Size(5, 5), 2, 2);
	// TODO: make adjustable via runtime
	cv::Canny(preprocessedImg, edges, 70, 110);

	// convert to colour for unity
	cv::cvtColor(edges, dst, CV_GRAY2BGRA);
}

std::vector<ProcessingOutput> ContourModule::processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info)
{
	cv::Mat rawMatLeft = cv::Mat(cv::Size(info.width, info.height), info.type, rawDataLeft);
	cv::Mat processedMatLeft;
	FindContours(rawMatLeft, processedMatLeft);

	cv::Mat rawMatRight = cv::Mat(cv::Size(info.width, info.height), info.type, rawDataRight);
	cv::Mat processedMatRight;
	FindContours(rawMatRight, processedMatRight);

	// copy data to separate arrays, since underlying data will be destroyed once cv::Mat is out of scope
	// TODO: verify?
	auto memSize = processedMatLeft.size().width * processedMatLeft.size().height * processedMatLeft.channels();

	ProcessingOutput outputLeft;
	outputLeft.type = ProcessingOutput::Type::left;
	outputLeft.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
	outputLeft.img = processedMatLeft;
	memcpy(outputLeft.data.get(), processedMatLeft.data, memSize);

	ProcessingOutput outputRight;
	outputRight.type = ProcessingOutput::Type::right;
	outputRight.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
	outputRight.img = processedMatRight;
	memcpy(outputRight.data.get(), processedMatRight.data, memSize);

	std::vector<ProcessingOutput> output;
	output.push_back(std::move(outputLeft));
	output.push_back(std::move(outputRight));
	return output;
}
