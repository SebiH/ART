#include "ARToolkitModule.h"

#include <AR/param.h>
#include <AR/arMulti.h>

using namespace ImageProcessing;

ARToolkitModule::ARToolkitModule()
{
	auto cparamLT_p = arParamLTCreate(nullptr, 0);
}

ARToolkitModule::~ARToolkitModule()
{

}

std::vector<ProcessingOutput> ARToolkitModule::processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info)
{
	//cv::Mat rawMatLeft = cv::Mat(cv::Size(info.width, info.height), info.type, rawDataLeft);
	//cv::Mat processedMatLeft;
	//FindContours(rawMatLeft, processedMatLeft);

	//cv::Mat rawMatRight = cv::Mat(cv::Size(info.width, info.height), info.type, rawDataRight);
	//cv::Mat processedMatRight;
	//FindContours(rawMatRight, processedMatRight);

	//// copy data to separate arrays, since underlying data will be destroyed once cv::Mat is out of scope
	//// TODO: verify?
	//auto memSize = processedMatLeft.size().width * processedMatLeft.size().height * processedMatLeft.channels();

	//ProcessingOutput outputLeft;
	//outputLeft.type = ProcessingOutput::Type::left;
	//outputLeft.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
	//outputLeft.img = processedMatLeft;
	//memcpy(outputLeft.data.get(), processedMatLeft.data, memSize);

	//ProcessingOutput outputRight;
	//outputRight.type = ProcessingOutput::Type::right;
	//outputRight.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
	//outputRight.img = processedMatRight;
	//memcpy(outputRight.data.get(), processedMatRight.data, memSize);

	std::vector<ProcessingOutput> output;
	//output.push_back(std::move(outputLeft));
	//output.push_back(std::move(outputRight));
	return output;
}
