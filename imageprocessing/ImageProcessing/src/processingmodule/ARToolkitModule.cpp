#include "ARToolkitModule.h"

#include <opencv2/imgproc.hpp>

#include "../util/UnityUtils.h"

using namespace ImageProcessing;

ARToolkitModule::ARToolkitModule()
{
}

ARToolkitModule::~ARToolkitModule()
{
	cleanup();
}

std::vector<ProcessingOutput> ARToolkitModule::processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info)
{
	if (!isInitialized)
	{
		initialize(info.width, info.height);
		isInitialized = true;
	}

	int arMarkerResult = arDetectMarker(arHandle, rawDataLeft);
	// TODO: if arMarkerResult < 0 ... errorhandling
	int arMarkerNum = arGetMarkerNum(arHandle);

	ProcessingOutput outputLeft;
	ProcessingOutput outputRight;

	if (arMarkerNum > 0)
	{
		auto markerInfo = arGetMarker(arHandle);

		cv::Mat imgLeft = cv::Mat(cv::Size(info.width, info.height), info.type, rawDataLeft);
		cv::circle(imgLeft, cv::Point(markerInfo->pos[0], markerInfo->pos[1]), 5, cv::Scalar(0, 0, 255, 1.0), 3);

		for (int i = 0; i < 4; i++)
		{
			auto cornerPos = cv::Point(markerInfo->vertex[i][0], markerInfo->vertex[i][1]);
			cv::circle(imgLeft, cornerPos, 3, cv::Scalar(255, 0, 0, 1.0), 3);
		}

		// copy data to separate arrays, since underlying data will be destroyed once cv::Mat is out of scope
		// TODO: verify?
		auto memSize = imgLeft.size().width * imgLeft.size().height * imgLeft.channels();

		outputLeft.type = ProcessingOutput::Type::left;
		outputLeft.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
		outputLeft.img = imgLeft;
		memcpy(outputLeft.data.get(), imgLeft.data, memSize);
	}
	else
	{
		outputLeft.type = ProcessingOutput::Type::left;
		outputLeft.data = std::unique_ptr<unsigned char[]>(new unsigned char[info.bufferSize]);
		memcpy(outputLeft.data.get(), rawDataLeft, info.bufferSize);
		outputLeft.img = cv::Mat(cv::Size(info.width, info.height), info.type, outputLeft.data.get());
	}

	//ProcessingOutput outputRight;
	outputRight.type = ProcessingOutput::Type::right;
	outputRight.data = std::unique_ptr<unsigned char[]>(new unsigned char[info.bufferSize]);
	memcpy(outputRight.data.get(), rawDataRight, info.bufferSize);
	outputRight.img = cv::Mat(cv::Size(info.width, info.height), info.type, outputRight.data.get());

	std::vector<ProcessingOutput> output;
	output.push_back(std::move(outputLeft));
	output.push_back(std::move(outputRight));
	return output;
}

void ARToolkitModule::initialize(int sizeX, int sizeY)
{
	if (arParamLoad("C:/code/data/calib_ovrvision_left.dat", 1, &cparam) < 0)
	{
		throw new std::exception("Failed to load AR parameters");
	}

	arParamChangeSize(&cparam, sizeX, sizeY, &cparam);
	arParamDisp(&cparam);

	if ((cparamLT_p = arParamLTCreate(&cparam, AR_PARAM_LT_DEFAULT_OFFSET)) == NULL)
	{
		throw new std::exception("Could not create arParamLT");
	}

	if ((arHandle = arCreateHandle(cparamLT_p)) == NULL)
	{
		throw new std::exception("Could not create arHandle");
	}

	if (arSetPixelFormat(arHandle, AR_PIXEL_FORMAT_BGRA) < 0)
	{
		throw new std::exception("Could not set ar pixel format");
	}

	if ((ar3DHandle = ar3DCreateHandle(&cparam)) == NULL)
	{
		throw new std::exception("Could not create ar 3d handle");
	}

	if ((arPattHandle = arPattCreateHandle()) == NULL)
	{
		throw new std::exception("Could not create arPattHandle");
	}


	if ((patt_id = arPattLoad(arPattHandle, "C:/code/data/hiro.patt")) < 0)
	{
		throw new std::exception("Could not load ARPatternHandle");
	}
	arPattAttach(arHandle, arPattHandle);

}

void ARToolkitModule::cleanup()
{
	arPattDeleteHandle(arPattHandle);
	ar3DDeleteHandle(&ar3DHandle);
	arDeleteHandle(arHandle);
	arParamLTFree(&cparamLT_p);
}

