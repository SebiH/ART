#include "OvrFrameProducer.h"

using namespace ImageProcessing;

OvrFrameProducer::OvrFrameProducer()
	: _isRunning(ATOMIC_VAR_INIT(false)),
	  _ovrCamera(std::unique_ptr<OVR::OvrvisionPro>(new OVR::OvrvisionPro())),
	  _mutex()
{
	//auto openSuccess = _ovrCamera->Open(0, OVR::OV_CAMVR_FULL);
	//auto openSuccess = _ovrCamera->Open(0, OVR::OV_CAM5MP_FHD);
	auto openSuccess = _ovrCamera->Open(0, OVR::Camprop::OV_CAMVR_QVGA);

	if (!openSuccess)
	{
		throw std::exception("Could not open camera!");
	}

	auto camWidth = _ovrCamera->GetCamWidth();
	auto camHeight = _ovrCamera->GetCamHeight();
	auto camDepth = _ovrCamera->GetCamPixelsize();
	_imgMemSize = camWidth * camHeight * camDepth;

	_dataLeft = std::unique_ptr<unsigned char[]>(new unsigned char[_imgMemSize]);
	_dataRight = std::unique_ptr<unsigned char[]>(new unsigned char[_imgMemSize]);

	_isRunning = true;
	_thread = std::thread(&OvrFrameProducer::run, this);
}


OvrFrameProducer::~OvrFrameProducer()
{
	if (std::atomic_exchange(&_isRunning, false))
	{
		_ovrCamera->Close();
		_thread.join();
	}
}


void OvrFrameProducer::poll(long &frameId, unsigned char *dataLeft, unsigned char *dataRight)
{
	std::unique_lock<std::mutex> lock(_mutex);

	_frameNotifier.wait(lock, [&]() { return frameId < _frameCounter; });
	frameId = _frameCounter;

	if (dataLeft != nullptr)
	{
		memcpy(dataLeft, _dataLeft.get(), _imgMemSize);
	}

	if (dataRight != nullptr)
	{
		memcpy(dataRight, _dataRight.get(), _imgMemSize);
	}
}


void OvrFrameProducer::run()
{
	// TODO ?
	time_t timeOfLastFrame = 0;
	time_t desiredFramerate = 1/60; // in seconds - better name?


	while (_isRunning)
	{
		// TODO: if isNextFrameAvailable
		query();
		Sleep(desiredFramerate * 1000);
	}
}

void OvrFrameProducer::query()
{
	_ovrCamera->PreStoreCamData(OVR::OV_CAMQT_DMS);

	auto dataLeft = _ovrCamera->GetCamImageBGRA(OVR::Cameye::OV_CAMEYE_LEFT);
	auto dataRight = _ovrCamera->GetCamImageBGRA(OVR::Cameye::OV_CAMEYE_RIGHT);

	{
		std::unique_lock<std::mutex> lock(_mutex);
		memcpy(_dataLeft.get(), dataLeft, _imgMemSize);
		memcpy(_dataRight.get(), dataRight, _imgMemSize);
		_frameCounter++;
		_frameNotifier.notify_all();
	}
}


OVR::OvrvisionPro* OvrFrameProducer::getCamera() const
{
	return _ovrCamera.get();
}

std::size_t OvrFrameProducer::getImageMemorySize() const
{
	return _imgMemSize;
}

long OvrFrameProducer::getCurrentFrameCount() const
{
	return _frameCounter;
}

std::condition_variable* OvrFrameProducer::getFrameNotifier()
{
	return &_frameNotifier;
}
