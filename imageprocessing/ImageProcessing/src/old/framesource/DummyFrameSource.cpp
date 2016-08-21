#include "DummyFrameSource.h"

#include <opencv2/imgproc.hpp>
#include <opencv2/highgui.hpp>

using namespace ImageProcessing;

DummyFrameSource::DummyFrameSource()
{
	// TODO: make parameter ...
	_img = cv::imread("C:\\code\\resources\\dummy_image_default.png");
	cv::cvtColor(_img, _img, cv::COLOR_RGB2RGBA);

	_imgInfo.channels = _img.channels();
	_imgInfo.width = _img.size().width;
	_imgInfo.height = _img.size().height;
	_imgInfo.bufferSize = _imgInfo.channels * _imgInfo.width * _imgInfo.height;
	_imgInfo.type = CV_8UC4;

	_imgBuffer = std::unique_ptr<unsigned char[]>(new unsigned char[_imgInfo.bufferSize]);
	std::memcpy(_imgBuffer.get(), _img.data, _imgInfo.bufferSize);
}


DummyFrameSource::~DummyFrameSource()
{

}


void DummyFrameSource::close()
{
}

ImageInfo DummyFrameSource::poll(long & frameId, unsigned char * bufferLeft, unsigned char * bufferRight)
{
	if (bufferLeft != nullptr)
	{
		memcpy(bufferLeft, _imgBuffer.get(), _imgInfo.bufferSize);
	}

	if (bufferRight != nullptr)
	{
		memcpy(bufferRight, _imgBuffer.get(), _imgInfo.bufferSize);
	}

	return _imgInfo;
}

std::size_t DummyFrameSource::getImageBufferSize() const
{
	return _imgInfo.bufferSize;
}

int DummyFrameSource::getFrameWidth() const
{
	return 0;
}

int DummyFrameSource::getFrameHeight() const
{
	return 0;
}

int DummyFrameSource::getFrameChannels() const
{
	return 0;
}

int DummyFrameSource::getCamExposure() const
{
	return 0;
}

void DummyFrameSource::setCamExposure(const int val) const
{
}

int DummyFrameSource::getCamGain() const
{
	return 0;
}

void DummyFrameSource::setCamGain(const int val) const
{
}

int DummyFrameSource::getCamBLC() const
{
	return 0;
}

void DummyFrameSource::setCamBLC(const int val) const
{
}

bool DummyFrameSource::getCamAutoWhiteBalance() const
{
	return false;
}

void DummyFrameSource::setCamAutoWhiteBalance(const bool val) const
{
}

int DummyFrameSource::getCamWhiteBalanceR() const
{
	return 0;
}

void DummyFrameSource::setCamWhiteBalanceR(const int val) const
{
}

int DummyFrameSource::getCamWhiteBalanceG() const
{
	return 0;
}

void DummyFrameSource::setCamWhiteBalanceG(const int val) const
{
}

int DummyFrameSource::getCamWhiteBalanceB() const
{
	return 0;
}

void DummyFrameSource::setCamWhiteBalanceB(const int val) const
{
}

int DummyFrameSource::getCamFps() const
{
	return 0;
}

bool DummyFrameSource::isOpen() const
{
	return true;
}
