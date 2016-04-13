#include "RawImageModule.h"

using namespace ImageProcessing;

RawImageModule::RawImageModule(int imgWidth, int imgHeight, int imgDepth)
	: _imgMemorySize(imgWidth * imgHeight * imgDepth),
	  _imgWidth(imgWidth),
	  _imgHeight(imgHeight)
{

}

RawImageModule::~RawImageModule()
{

}


std::vector<ProcessingOutput> RawImageModule::processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight)
{
	ProcessingOutput outputLeft;
	outputLeft.type = ProcessingOutput::Type::left;
	outputLeft.data = std::unique_ptr<unsigned char[]>(new unsigned char[_imgMemorySize]);
	memcpy(outputLeft.data.get(), rawDataLeft, _imgMemorySize);
	outputLeft.img = cv::Mat(cv::Size(_imgWidth, _imgHeight), CV_8UC4, outputLeft.data.get());


	ProcessingOutput outputRight;
	outputRight.type = ProcessingOutput::Type::right;
	outputRight.data = std::unique_ptr<unsigned char[]>(new unsigned char[_imgMemorySize]);
	memcpy(outputRight.data.get(), rawDataRight, _imgMemorySize);
	outputRight.img = cv::Mat(cv::Size(_imgWidth, _imgHeight), CV_8UC4, outputRight.data.get());

	std::vector<ProcessingOutput> output;
	output.push_back(std::move(outputLeft));
	output.push_back(std::move(outputRight));

	//return std::vector<ProcessingOutput> { outputLeft, outputRight };
	return output;
}
