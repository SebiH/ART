#include "cameras/OvrvisionCameraSource.h"

using namespace ImageProcessing;

OvrvisionCameraSource::OvrvisionCameraSource(OVR::Camprop quality, OVR::Camqt processMode)
{
}

OvrvisionCameraSource::~OvrvisionCameraSource()
{
}

ImageMetaData OvrvisionCameraSource::GetCurrentFrame()
{
	return ImageMetaData();
}


bool OvrvisionCameraSource::IsOpen() const
{
	return false;
}


void OvrvisionCameraSource::Open()
{
}

void OvrvisionCameraSource::Close()
{
}




float OvrvisionCameraSource::GetCamFocalPoint() const
{
	return 0.0f;
}

float OvrvisionCameraSource::GetHMDRightGap(const int at) const
{
	return 0.0f;
}

void OvrvisionCameraSource::SetProcessingMode(const OVR::Camqt mode)
{
}

int OvrvisionCameraSource::GetProcessingMode() const
{
	return 0;
}






int OvrvisionCameraSource::GetFrameWidth() const
{
	return 0;
}

int OvrvisionCameraSource::GetFrameHeight() const
{
	return 0;
}

int OvrvisionCameraSource::GetFrameChannels() const
{
	return 0;
}

int OvrvisionCameraSource::GetCamExposure() const
{
	return 0;
}

void OvrvisionCameraSource::SetCamExposure(const int val) const
{
}

int OvrvisionCameraSource::GetCamGain() const
{
	return 0;
}

void OvrvisionCameraSource::SetCamGain(const int val) const
{
}

int OvrvisionCameraSource::GetCamBLC() const
{
	return 0;
}

void OvrvisionCameraSource::SetCamBLC(const int val) const
{
}

bool OvrvisionCameraSource::GetCamAutoWhiteBalance() const
{
	return false;
}

void OvrvisionCameraSource::SetCamAutoWhiteBalance(const bool val) const
{
}

int OvrvisionCameraSource::GetCamWhiteBalanceR() const
{
	return 0;
}

void OvrvisionCameraSource::SetCamWhiteBalanceR(const int val) const
{
}

int OvrvisionCameraSource::GetCamWhiteBalanceG() const
{
	return 0;
}

void OvrvisionCameraSource::SetCamWhiteBalanceG(const int val) const
{
}

int OvrvisionCameraSource::GetCamWhiteBalanceB() const
{
	return 0;
}

void OvrvisionCameraSource::SetCamWhiteBalanceB(const int val) const
{
}

int OvrvisionCameraSource::GetCamFps() const
{
	return 0;
}
