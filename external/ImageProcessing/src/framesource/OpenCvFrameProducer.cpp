#include "OpenCVFrameProducer.h"

#include <Windows.h>
#include <opencv2/imgproc.hpp>

using namespace ImageProcessing;

OpenCVFrameProducer::OpenCVFrameProducer()
	: _isRunning(ATOMIC_VAR_INIT(false)),
	  _camera(std::make_unique<cv::VideoCapture>(0)),
	  _mutex()
{
	int camWidth = static_cast<int>(_camera->get(cv::CAP_PROP_FRAME_WIDTH));
	int camHeight = static_cast<int>(_camera->get(cv::CAP_PROP_FRAME_HEIGHT));
	int camDepth = getFrameChannels();
	_imgBufferSize = camWidth * camHeight * camDepth;
	_imgInfo = ImageInfo(camWidth, camHeight, camDepth, CV_8UC4);

	_dataLeft = std::unique_ptr<unsigned char[]>(new unsigned char[_imgBufferSize]);
	_dataRight = std::unique_ptr<unsigned char[]>(new unsigned char[_imgBufferSize]);

	_isRunning = true;
	_thread = std::thread(&OpenCVFrameProducer::run, this);
}


OpenCVFrameProducer::~OpenCVFrameProducer()
{
	if (std::atomic_exchange(&_isRunning, false))
	{
		_camera->release();
		_camera.release();
		_thread.join();
	}
}


ImageInfo OpenCVFrameProducer::poll(long &frameId, unsigned char *bufferLeft, unsigned char *bufferRight)
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


void OpenCVFrameProducer::run()
{
	// TODO ?
	time_t timeOfLastFrame = 0;
	float desiredFramerate = 1/30.f; // in seconds - better name?


	while (_isRunning)
	{
		// TODO: if isNextFrameAvailable
		query();
		Sleep(desiredFramerate * 1000);
	}
}

void OpenCVFrameProducer::query()
{
	cv::Mat frame;
	(*_camera) >> frame;

	cv::Mat converted;
	cv::cvtColor(frame, converted, CV_BGR2BGRA);
	converted.copyTo(frame);

	std::unique_lock<std::mutex> lock(_mutex);
	memcpy(_dataLeft.get(), frame.data, _imgBufferSize);
	memcpy(_dataRight.get(), frame.data, _imgBufferSize);
	_frameCounter++;
	_frameNotifier.notify_all();
}


std::size_t OpenCVFrameProducer::getImageBufferSize() const
{
	return _imgBufferSize;
}

// camera properties
int OpenCVFrameProducer::getFrameWidth() const
{
	return static_cast<int>(_camera->get(cv::CAP_PROP_FRAME_WIDTH));
}

int OpenCVFrameProducer::getFrameHeight() const
{
	return static_cast<int>(_camera->get(cv::CAP_PROP_FRAME_HEIGHT));
}

int OpenCVFrameProducer::getFrameChannels() const
{
	return 4;
}

float OpenCVFrameProducer::getCamExposure() const
{
	return static_cast<float>(_camera->get(cv::CAP_PROP_EXPOSURE));
}


void OpenCVFrameProducer::setCamExposure(float val) const
{
	_camera->set(cv::CAP_PROP_EXPOSURE, val);
}


float OpenCVFrameProducer::getCamGain() const
{
	return static_cast<float>(_camera->get(cv::CAP_PROP_GAIN));
}


void OpenCVFrameProducer::setCamGain(float val) const
{
	_camera->set(cv::CAP_PROP_GAIN, val);
}

bool OpenCVFrameProducer::isOpen() const
{
	return _camera->isOpened();
}
