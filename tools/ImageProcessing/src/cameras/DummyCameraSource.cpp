#include "cameras/DummyCameraSource.h"

#include <chrono>
#include <thread>
#include <opencv2/highgui.hpp>
#include <opencv2/imgproc.hpp>

#include "utils/Logger.h"

using namespace ImageProcessing;

DummyCameraSource::DummyCameraSource(std::string filename)
	: img_(cv::imread(filename))
{
	if (img_.channels() == 3)
	{
		// convert back to 4-channel BGRA for easier unity handling
		cv::cvtColor(img_, img_, CV_BGR2BGRA);
	}
	else if (img_.channels() != GetFrameChannels())
	{
		DebugLog("Camera provided unknown amount of channels");
		//throw std::exception("Camera provided unexpected amount of channels");
	}
}

DummyCameraSource::~DummyCameraSource()
{

}


void DummyCameraSource::PrepareNextFrame()
{
	if (!is_first_image_)
	{
		std::this_thread::sleep_for(std::chrono::milliseconds(10));
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
	// image is converted to 4 channels to match textures in unity
	return 4;
}

float DummyCameraSource::GetFocalLength() const
{
	return 1.f;
}

void DummyCameraSource::SetProperties(const nlohmann::json &json_config)
{
}

nlohmann::json DummyCameraSource::GetProperties() const
{
	return nlohmann::json();
}
