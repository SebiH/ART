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
	if (!is_first_image_)
	{
		std::this_thread::sleep_for(std::chrono::milliseconds(500));
	}
	else
	{
		is_first_image_ = false;
	}
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

void DummyCameraSource::SetProperties(const nlohmann::json &json_config)
{
}

nlohmann::json DummyCameraSource::GetProperties() const
{
	return nlohmann::json();
}
