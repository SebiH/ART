#include "RawImageModule.h"

using namespace ImageProcessing;

RawImageModule::RawImageModule(int imgWidth, int imgHeight, int imgDepth)
	: _imgMemorySize(imgWidth * imgHeight * imgDepth)
{

}

RawImageModule::~RawImageModule()
{

}


std::vector<std::unique_ptr<unsigned char[]>> RawImageModule::processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight)
{
	std::vector<std::unique_ptr<unsigned char[]>> output;

	std::unique_ptr<unsigned char[]> dataLeft(new unsigned char[_imgMemorySize]);
	memcpy(dataLeft.get(), rawDataLeft, _imgMemorySize);
	output.push_back(std::move(dataLeft));

	std::unique_ptr<unsigned char[]> dataRight(new unsigned char[_imgMemorySize]);
	memcpy(dataRight.get(), rawDataRight, _imgMemorySize);
	output.push_back(std::move(dataRight));

	return output;
}
