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

float EmptyCameraSource::GetFocalLength() const
{
	return 0.1f;
}

void EmptyCameraSource::SetProperties(const nlohmann::json &json_config)
{
}

nlohmann::json EmptyCameraSource::GetProperties() const
{
	return nlohmann::json();
}
