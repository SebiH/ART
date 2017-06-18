#include "cameras/EmptyCameraSource.h"

#include <chrono>
#include <thread>

using namespace ImageProcessing;

void EmptyCameraSource::PrepareNextFrame()
{
	std::this_thread::sleep_for(std::chrono::milliseconds(10));
}

void EmptyCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
	auto buffer_size = GetFrameWidth() * GetFrameHeight() * GetFrameChannels();
	std::memset(left_buffer, rand() % 255, buffer_size);
	std::memset(right_buffer, rand() % 255, buffer_size);
}

void EmptyCameraSource::Open()
{
}

void EmptyCameraSource::Close()
{
}

bool EmptyCameraSource::IsOpen() const
{
	return true;
}

int EmptyCameraSource::GetFrameWidth() const
{
	return 1920 * 2;
}

int EmptyCameraSource::GetFrameHeight() const
{
	return 1080 * 2;
}

int EmptyCameraSource::GetFrameChannels() const
{
	return 4;
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
