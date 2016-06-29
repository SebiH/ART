#include "LeapFrameSource.h"

#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include <Windows.h>

using namespace ImageProcessing;

LeapFrameSource::LeapFrameSource()
	: _isRunning(ATOMIC_VAR_INIT(false)),
	_camera(std::make_unique<Leap::Controller>()),
	_mutex()
{
	_camera->setPolicy(Leap::Controller::POLICY_IMAGES);

	// avoid endless loop in case of device not connected
	int retries = 0;
	int maxRetries = 1000;

	// first few times the leap controller can return a zero-sized image,
	// therefore repeat until we have something useful
	while (retries < maxRetries)
	{
		retries++;

		auto images = _camera->images();

		int camWidth = images[0].width();
		int camHeight = images[0].height();
		//int camDepth = images[0].bytesPerPixel();
		int camDepth = 4; // enforce RGBA later on

		_imgBufferSize = camWidth * camHeight * camDepth;

		if (_imgBufferSize == 0)
		{
			Sleep(100);
		}
		else
		{
			_imgInfo = ImageInfo(camWidth, camHeight, camDepth, CV_8UC4);
			_dataLeft = std::unique_ptr<unsigned char[]>(new unsigned char[_imgBufferSize]);
			_dataRight = std::unique_ptr<unsigned char[]>(new unsigned char[_imgBufferSize]);
			break;
		}
	}

	_isRunning = true;
	_thread = std::thread(&LeapFrameSource::run, this);
}


LeapFrameSource::~LeapFrameSource()
{
	this->close();
}


void LeapFrameSource::close()
{
	if (std::atomic_exchange(&_isRunning, false))
	{
		_camera.release();
		_thread.join();
	}
}


ImageInfo LeapFrameSource::poll(long &frameId, unsigned char *bufferLeft, unsigned char *bufferRight)
{
	std::unique_lock<std::mutex> lock(_mutex);

	_frameNotifier.wait(lock, [&]() { return frameId < _frameCounter; });
	frameId = _frameCounter;

	if (bufferLeft != nullptr)
	{
		memcpy(bufferLeft, _dataLeft.get(), _imgBufferSize);
	}

	if (bufferRight != nullptr)
	{
		memcpy(bufferRight, _dataRight.get(), _imgBufferSize);
	}

	return _imgInfo;
}


void LeapFrameSource::run()
{
	// TODO ?
	time_t timeOfLastFrame = 0;
	float desiredFramerate = 1 / 30.f; // in seconds - better name?

	while (_isRunning)
	{
		// TODO: if isNextFrameAvailable
		query();
		Sleep(desiredFramerate * 1000);
	}
}

void LeapFrameSource::query()
{
	auto images = _camera->images();

	auto memSize = images[0].width() * images[0].height() * images[0].bytesPerPixel();

	if (memSize == 0)
	{
		return;
	}

	std::unique_ptr<unsigned char[]> tempLeftData(new unsigned char[memSize]);
	memcpy(tempLeftData.get(), images[0].data(), memSize);

	std::unique_ptr<unsigned char[]> tempRightData(new unsigned char[memSize]);
	memcpy(tempRightData.get(), images[1].data(), memSize);

	cv::Mat grayLeft(cv::Size(_imgInfo.width, _imgInfo.height), CV_8UC1, tempLeftData.get());
	cv::Mat colourLeft, outputLeft;
	cv::cvtColor(grayLeft, colourLeft, cv::COLOR_GRAY2BGRA);
	colourLeft.copyTo(outputLeft);

	cv::Mat grayRight(cv::Size(_imgInfo.width, _imgInfo.height), CV_8UC1, tempRightData.get());
	cv::Mat colourRight, outputRight;
	cv::cvtColor(grayRight, colourRight, cv::COLOR_GRAY2BGRA);
	colourRight.copyTo(outputRight);

	std::unique_lock<std::mutex> lock(_mutex);
	memcpy(_dataLeft.get(), outputLeft.data, _imgBufferSize);
	memcpy(_dataRight.get(), outputRight.data, _imgBufferSize);
	_frameCounter++;
	_frameNotifier.notify_all();
}


std::size_t LeapFrameSource::getImageBufferSize() const
{
	return _imgBufferSize;
}

// camera properties
int LeapFrameSource::getFrameWidth() const
{
	return _imgInfo.width;
}

int LeapFrameSource::getFrameHeight() const
{
	return _imgInfo.height;
}

int LeapFrameSource::getFrameChannels() const
{
	return _imgInfo.channels;
}

int LeapFrameSource::getCamExposure() const
{
	// not possible?
	return 0;
}


void LeapFrameSource::setCamExposure(int val) const
{
	// not possible?
}


int LeapFrameSource::getCamGain() const
{
	// not possible?
	return 0;
}


void LeapFrameSource::setCamGain(int val) const
{
	// not possible?
}

bool LeapFrameSource::isOpen() const
{
	return _camera->isConnected();
}


int LeapFrameSource::getCamBLC() const
{
	// Not implemented/possible(?)
	return 0;
}

void LeapFrameSource::setCamBLC(const int val) const
{
	// Not implemented/possible(?)
}

bool LeapFrameSource::getCamAutoWhiteBalance() const
{
	// Not implemented/possible(?)
	return true;
}

void LeapFrameSource::setCamAutoWhiteBalance(const bool val) const
{
	// Not implemented/possible(?)
}

int LeapFrameSource::getCamWhiteBalanceR() const
{
	// Not implemented/possible(?)
	return 0;
}

void LeapFrameSource::setCamWhiteBalanceR(const int val) const
{
	// Not implemented/possible(?)
}

int LeapFrameSource::getCamWhiteBalanceG() const
{
	// Not implemented/possible(?)
	return 0;
}

void LeapFrameSource::setCamWhiteBalanceG(const int val) const
{
	// Not implemented/possible(?)
}

int LeapFrameSource::getCamWhiteBalanceB() const
{
	// Not implemented/possible(?)
	return 0;
}

void LeapFrameSource::setCamWhiteBalanceB(const int val) const
{
	// Not implemented/possible(?)
}
