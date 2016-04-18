#include "RawImageModule.h"

using namespace ImageProcessing;

RawImageModule::RawImageModule()
{

}

RawImageModule::~RawImageModule()
{

}


std::vector<ProcessingOutput> RawImageModule::processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info)
{
	ProcessingOutput outputLeft;
	outputLeft.type = ProcessingOutput::Type::left;
	outputLeft.data = std::unique_ptr<unsigned char[]>(new unsigned char[info.bufferSize]);
	memcpy(outputLeft.data.get(), rawDataLeft, info.bufferSize);
	outputLeft.img = cv::Mat(cv::Size(info.width, info.height), info.type, outputLeft.data.get());


	ProcessingOutput outputRight;
	outputRight.type = ProcessingOutput::Type::right;
	outputRight.data = std::unique_ptr<unsigned char[]>(new unsigned char[info.bufferSize]);
	memcpy(outputRight.data.get(), rawDataRight, info.bufferSize);
	outputRight.img = cv::Mat(cv::Size(info.width, info.height), info.type, outputRight.data.get());

	std::vector<ProcessingOutput> output;
	output.push_back(std::move(outputLeft));
	output.push_back(std::move(outputRight));

	//return std::vector<ProcessingOutput> { outputLeft, outputRight };
	return output;
}
