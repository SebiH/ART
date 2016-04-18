#include "RoiModule.h"

#include <utility>

using namespace ImageProcessing;

RoiModule::RoiModule(int camWidth, int camHeight)
	: _maxWidth(camWidth),
	  _maxHeight(camHeight)
{
	_roi = cv::Rect(0, 0, camWidth, camHeight);
}

RoiModule::~RoiModule()
{
}

std::vector<ProcessingOutput> RoiModule::processImage(unsigned char * rawDataLeft, unsigned char * rawDataRight)
{
	cv::Rect roi;

	{
		std::lock_guard<std::mutex> guard(_roiMutex);
		roi = _roi;
	}

	cv::Mat rawMatLeft = cv::Mat(cv::Size(_maxWidth, _maxHeight), CV_8UC3, rawDataLeft);
	cv::Mat rawRoiLeft = cv::Mat(rawMatLeft, roi);
	cv::Mat pureRoiLeft;
	rawRoiLeft.copyTo(pureRoiLeft);


	cv::Mat rawMatRight = cv::Mat(cv::Size(_maxWidth, _maxHeight), CV_8UC3, rawDataRight);
	cv::Mat rawRoiRight = cv::Mat(rawMatRight, roi);
	cv::Mat pureRoiRight;
	rawRoiRight.copyTo(pureRoiRight);

	// copy data to separate arrays, since underlying data will be destroyed once cv::Mat is out of scope
	// TODO: verify?
	auto memSize = roi.width * roi.height * 3;

	ProcessingOutput outputLeft;
	outputLeft.type = ProcessingOutput::Type::left;
	outputLeft.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
	outputLeft.img = pureRoiLeft;
	memcpy(outputLeft.data.get(), rawRoiLeft.data, memSize);

	ProcessingOutput outputRight;
	outputRight.type = ProcessingOutput::Type::right;
	outputRight.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
	outputRight.img = pureRoiRight;
	memcpy(outputRight.data.get(), rawRoiRight.data, memSize);

	std::vector<ProcessingOutput> output;
	output.push_back(std::move(outputLeft));
	output.push_back(std::move(outputRight));

	//return std::vector<ProcessingOutput> { outputLeft, outputRight };
	return output;
}


void RoiModule::setRegion(cv::Rect roi)
{
	std::lock_guard<std::mutex> guard(_roiMutex);
	_roi = roi;
}
