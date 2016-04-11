#include "OvrFrameProducer.h"

#include <mutex>

using namespace ImageProcessing;

OvrFrameProducer::OvrFrameProducer()
	: _isRunning(ATOMIC_VAR_INIT(false)),
	  _ovrCamera(std::unique_ptr<OVR::OvrvisionPro>(new OVR::OvrvisionPro())),
	  _mutex()
{
	auto openSuccess = _ovrCamera->Open(0, OVR::OV_CAMVR_FULL);

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


void OvrFrameProducer::poll(unsigned char *dataLeft, unsigned char *dataRight)
{
	std::lock_guard<std::mutex> lock(_mutex);
	memcpy(dataLeft, _dataLeft.get(), _imgMemSize);
	memcpy(dataRight, _dataRight.get(), _imgMemSize);
}


void OvrFrameProducer::run()
{
	// TODO ?
	time_t timeOfLastFrame = 0;
	time_t desiredFramerate = 1/60; // in ms - better name?


	while (_isRunning)
	{
		// TODO: if isNextFrameAvailable
		query();
		Sleep(10);
	}
}

void OvrFrameProducer::query()
{
	_ovrCamera->PreStoreCamData(OVR::OV_CAMQT_DMS);

	auto dataLeft = _ovrCamera->GetCamImageBGRA(OVR::Cameye::OV_CAMEYE_LEFT);
	auto dataRight = _ovrCamera->GetCamImageBGRA(OVR::Cameye::OV_CAMEYE_RIGHT);

	std::lock_guard<std::mutex> lock(_mutex);
	memcpy(_dataLeft.get(), dataLeft, _imgMemSize);
	memcpy(_dataRight.get(), dataRight, _imgMemSize);

	// TODO: eventbased system that triggers other threads to poll() new data?
}


const OVR::OvrvisionPro* OvrFrameProducer::getCamera()
{
	return _ovrCamera.get();
}

const std::size_t OvrFrameProducer::getImageMemorySize()
{
	return _imgMemSize;
}
