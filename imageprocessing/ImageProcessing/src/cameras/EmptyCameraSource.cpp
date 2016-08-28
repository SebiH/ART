#include "cameras/EmptyCameraSource.h"

#include <chrono>
#include <thread>

using namespace ImageProcessing;

void EmptyCameraSource::PrepareNextFrame()
{
	std::this_thread::sleep_for(std::chrono::seconds(1));
}

void EmptyCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
	auto buffer_size = GetFrameWidth() * GetFrameHeight() * GetFrameChannels();
	std::memset(left_buffer, 0, buffer_size);
	std::memset(right_buffer, 0, buffer_size);
}

void EmptyCameraSource::Open()
{
}

void EmptyCameraSource::Close()
{
}

bool EmptyCameraSource::IsOpen() const
{
	return false;
}

int EmptyCameraSource::GetFrameWidth() const
{
	return 640;
}

int EmptyCameraSource::GetFrameHeight() const
{
	return 480;
}

int EmptyCameraSource::GetFrameChannels() const
{
	return 3;
}

int EmptyCameraSource::GetCamExposure() const
{
	return 0;
}

void EmptyCameraSource::SetCamExposure(const int val) const
{
}

void EmptyCameraSource::SetCamExposurePerSec(const float val) const
{
}

int EmptyCameraSource::GetCamGain() const
{
	return 0;
}

void EmptyCameraSource::SetCamGain(const int val) const
{
}

int EmptyCameraSource::GetCamBLC() const
{
	return 0;
}

void EmptyCameraSource::SetCamBLC(const int val) const
{
}

bool EmptyCameraSource::GetCamAutoWhiteBalance() const
{
	return false;
}

void EmptyCameraSource::SetCamAutoWhiteBalance(const bool val) const
{
}

int EmptyCameraSource::GetCamWhiteBalanceR() const
{
	return 0;
}

void EmptyCameraSource::SetCamWhiteBalanceR(const int val) const
{
}

int EmptyCameraSource::GetCamWhiteBalanceG() const
{
	return 0;
}

void EmptyCameraSource::SetCamWhiteBalanceG(const int val) const
{
}

int EmptyCameraSource::GetCamWhiteBalanceB() const
{
	return 0;
}

void EmptyCameraSource::SetCamWhiteBalanceB(const int val) const
{
}

int EmptyCameraSource::GetCamFps() const
{
	return 1;
}
