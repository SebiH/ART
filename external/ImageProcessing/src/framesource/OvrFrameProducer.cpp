#include "OvrFrameProducer.h"

#include <opencv2/core.hpp>

using namespace ImageProcessing;

OvrFrameProducer::OvrFrameProducer()
	: _isRunning(ATOMIC_VAR_INIT(false)),
	  _ovrCamera(std::unique_ptr<OVR::OvrvisionPro>(new OVR::OvrvisionPro())),
	  _mutex()
{
	//auto openSuccess = _ovrCamera->Open(0, OVR::OV_CAMVR_FULL);
	//auto openSuccess = _ovrCamera->Open(0, OVR::OV_CAM5MP_FHD);
	//auto openSuccess = _ovrCamera->Open(0, OVR::Camprop::OV_CAMVR_QVGA);
	auto openSuccess = _ovrCamera->Open(0, OVR::Camprop::OV_CAMVR_VGA);

	if (!openSuccess)
	{
		throw std::exception("Could not open camera!");
	}

	auto camWidth = _ovrCamera->GetCamWidth();
	auto camHeight = _ovrCamera->GetCamHeight();
	auto camDepth = _ovrCamera->GetCamPixelsize();
	_imgBufferSize = camWidth * camHeight * camDepth;
	_imgInfo = ImageInfo(camWidth, camHeight, camDepth, CV_8UC4);

	_dataLeft = std::unique_ptr<unsigned char[]>(new unsigned char[_imgBufferSize]);
	_dataRight = std::unique_ptr<unsigned char[]>(new unsigned char[_imgBufferSize]);

	_isRunning = true;
	_thread = std::thread(&OvrFrameProducer::run, this);
}


OvrFrameProducer::~OvrFrameProducer()
{
	this->close();
}

void OvrFrameProducer::close()
{
	if (std::atomic_exchange(&_isRunning, false))
	{
		_ovrCamera->Close();
		_thread.join();
	}
}

ImageInfo OvrFrameProducer::poll(long &frameId, unsigned char *bufferLeft, unsigned char *bufferRight)
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


void OvrFrameProducer::run()
{
	// TODO ?
	time_t timeOfLastFrame = 0;
	float desiredFramerate = 1/60.f; // in seconds - better name?


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
		memcpy(_dataLeft.get(), dataLeft, _imgBufferSize);
		memcpy(_dataRight.get(), dataRight, _imgBufferSize);
		_frameCounter++;
		_frameNotifier.notify_all();
	}
}


std::size_t OvrFrameProducer::getImageBufferSize() const
{
	return _imgBufferSize;
}


int OvrFrameProducer::getFrameWidth() const
{
	return _ovrCamera->GetCamWidth();
}


int OvrFrameProducer::getFrameHeight() const
{
	return _ovrCamera->GetCamHeight();
}

int OvrFrameProducer::getFrameChannels() const
{
	return 4;
}


int OvrFrameProducer::getCamExposure() const
{
	return _ovrCamera->GetCameraExposure();
}


void OvrFrameProducer::setCamExposure(int val) const
{
	_ovrCamera->SetCameraExposure(val);
}


int OvrFrameProducer::getCamGain() const
{
	return _ovrCamera->GetCameraGain();
}


void OvrFrameProducer::setCamGain(int val) const
{
	_ovrCamera->SetCameraGain(val);
}

bool OvrFrameProducer::isOpen() const
{
	return _ovrCamera->isOpen();
}

int OvrFrameProducer::getCamBLC() const
{
	return _ovrCamera->GetCameraBLC();
}

void OvrFrameProducer::setCamBLC(const int val) const
{
	_ovrCamera->SetCameraBLC(val);
}

bool OvrFrameProducer::getCamAutoWhiteBalance() const
{
	return _ovrCamera->GetCameraWhiteBalanceAuto();
}

void OvrFrameProducer::setCamAutoWhiteBalance(const bool val) const
{
	_ovrCamera->SetCameraWhiteBalanceAuto(val);
}

int OvrFrameProducer::getCamWhiteBalanceR() const
{
	return _ovrCamera->GetCameraWhiteBalanceR();
}

void OvrFrameProducer::setCamWhiteBalanceR(const int val) const
{
	_ovrCamera->SetCameraWhiteBalanceR(val);
}

int OvrFrameProducer::getCamWhiteBalanceG() const
{
	return _ovrCamera->GetCameraWhiteBalanceG();
}

void OvrFrameProducer::setCamWhiteBalanceG(const int val) const
{
	_ovrCamera->SetCameraWhiteBalanceG(val);
}

int OvrFrameProducer::getCamWhiteBalanceB() const
{
	return _ovrCamera->GetCameraWhiteBalanceB();
}

void OvrFrameProducer::setCamWhiteBalanceB(const int val) const
{
	_ovrCamera->SetCameraWhiteBalanceB(val);
}

int OvrFrameProducer::getCamFps() const
{
	return _ovrCamera->GetCamFramerate();
}

float OvrFrameProducer::getCamFocalPoint() const
{
	// get float point??
	//ovGetFocalPoint() * 0.001f;	//1/100
	return _ovrCamera->GetCamFocalPoint();
}

// ???
float OvrFrameProducer::getHMDRightGap(const int at) const
{
	return _ovrCamera->GetHMDRightGap(at);
}
