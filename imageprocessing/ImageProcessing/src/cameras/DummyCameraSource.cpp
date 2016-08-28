#include "cameras/DummyCameraSource.h"

#include <chrono>
#include <thread>
#include <opencv2/highgui.hpp>

using namespace ImageProcessing;

DummyCameraSource::DummyCameraSource(std::string filename)
	: img_(cv::imread(filename))
{

}

DummyCameraSource::~DummyCameraSource()
{

}


void DummyCameraSource::PrepareNextFrame()
{
	std::this_thread::sleep_for(std::chrono::milliseconds(500));
}


void DummyCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
	auto buffer_size = img_.channels() * img_.size().width * img_.size().height;
	memcpy(left_buffer, img_.data, buffer_size);
	memcpy(right_buffer, img_.data, buffer_size);
}

void DummyCameraSource::Open()
{
}

void DummyCameraSource::Close()
{
}

bool DummyCameraSource::IsOpen() const
{
	return true;
}

int DummyCameraSource::GetFrameWidth() const
{
	return img_.size().width;
}

int DummyCameraSource::GetFrameHeight() const
{
	return img_.size().height;
}

int DummyCameraSource::GetFrameChannels() const
{
	return img_.channels();
}

int DummyCameraSource::GetCamExposure() const
{
	return 0;
}

void DummyCameraSource::SetCamExposure(const int val) const
{
}

int DummyCameraSource::GetCamGain() const
{
	return 0;
}

void DummyCameraSource::SetCamGain(const int val) const
{
}

int DummyCameraSource::GetCamBLC() const
{
	return 0;
}

void DummyCameraSource::SetCamBLC(const int val) const
{
}

bool DummyCameraSource::GetCamAutoWhiteBalance() const
{
	return false;
}

void DummyCameraSource::SetCamAutoWhiteBalance(const bool val) const
{
}

int DummyCameraSource::GetCamWhiteBalanceR() const
{
	return 0;
}

void DummyCameraSource::SetCamWhiteBalanceR(const int val) const
{
}

int DummyCameraSource::GetCamWhiteBalanceG() const
{
	return 0;
}

void DummyCameraSource::SetCamWhiteBalanceG(const int val) const
{
}

int DummyCameraSource::GetCamWhiteBalanceB() const
{
	return 0;
}

void DummyCameraSource::SetCamWhiteBalanceB(const int val) const
{
}

int DummyCameraSource::GetCamFps() const
{
	return 1;
}

void ImageProcessing::DummyCameraSource::SetCamExposurePerSec(const float val) const
{
}
