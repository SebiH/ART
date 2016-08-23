#include "cameras\ActiveCamera.h"

#include <algorithm>

#include "utils/Logger.h"

using namespace ImageProcessing;

ActiveCamera::ActiveCamera()
	: _camSourceMutex(),
	  _frameDataMutex(),
	  _frameIdMutex(),
	  _frameNotifier()
{

}

ActiveCamera::~ActiveCamera()
{
}


void ActiveCamera::ResizeBuffers(const FrameMetaData &newData)
{
	auto bufferSize = newData.GetBufferSize();
	_frameDataLeft = std::make_unique<unsigned char[]>(bufferSize);
	_frameDataRight = std::make_unique<unsigned char[]>(bufferSize);
}

void ActiveCamera::FetchFrame()
{
	// Use copy (instead of member) to avoid lock over whole FetchFrame operation
	auto cameraSrc = GetSource();

	if (cameraSrc.get() == nullptr || !cameraSrc->IsOpen())
	{
		return;
	}

	try
	{
		cameraSrc->GrabFrame(_frameDataLeft.get(), _frameDataRight.get());
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Could not grab new frame: ") + e.what());
	}


}



void ActiveCamera::SetSource(const std::shared_ptr<ICameraSource> &cam)
{
	std::unique_lock<std::mutex> lock(_camSourceMutex);
	_cameraSource = cam;

	if (cam.get() != nullptr)
	{
		auto newFrameData = cam->GetFrameMetaData();

		if (_currentFrameMetaData != newFrameData)
		{
			// fire OnSourceChanged events, resize buffers, re-process FrameMetaData
			_currentFrameMetaData = newFrameData;
			ResizeBuffers(newFrameData);
		}
	}

}

std::shared_ptr<ICameraSource> ActiveCamera::GetSource()
{
	std::unique_lock<std::mutex> lock(_camSourceMutex);
	return _cameraSource;
}


void ActiveCamera::WaitForNewFrame(int consumerFrameId)
{
	std::unique_lock<std::mutex> lock(_frameIdMutex);
	_frameNotifier.wait(lock, [&]() { return _frameCounter > consumerFrameId; });
}

void ActiveCamera::WriteFrame(unsigned char *leftBuffer, unsigned char *rightBuffer)
{
	std::unique_lock<std::mutex> lock(_frameDataMutex);

	if (leftBuffer != nullptr)
	{
		std::memcpy(leftBuffer, _frameDataLeft.get(), 0);
	}

	if (rightBuffer != nullptr)
	{
		std::memcpy(rightBuffer, _frameDataRight.get(), 0);
	}
}



FrameMetaData ActiveCamera::GetCurrentFrameData()
{
	std::unique_lock<std::mutex> lock(_frameDataMutex);
	return _currentFrameMetaData;
}
