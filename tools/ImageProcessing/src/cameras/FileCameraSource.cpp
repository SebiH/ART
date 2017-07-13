#include "cameras/FileCameraSource.h"

#include <chrono>
#include <thread>
#include <opencv2/highgui.hpp>
#include <opencv2/imgproc.hpp>

#include "utils/Logger.h"

using namespace ImageProcessing;

FileCameraSource::FileCameraSource(std::string filename)
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

FileCameraSource::~FileCameraSource()
{

}


void FileCameraSource::PrepareNextFrame()
{
	if (!is_first_image_)
	{
		std::this_thread::sleep_for(std::chrono::milliseconds(50));
	}
	else
	{
		is_first_image_ = false;
	}
}


void FileCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
	auto buffer_size = img_.channels() * img_.size().width * img_.size().height;
	memcpy(left_buffer, img_.data, buffer_size);
	memcpy(right_buffer, img_.data, buffer_size);
}

void FileCameraSource::Open()
{
}

void FileCameraSource::Close()
{
}

bool FileCameraSource::IsOpen() const
{
	return true;
}

int FileCameraSource::GetFrameWidth() const
{
	return img_.size().width;
}

int FileCameraSource::GetFrameHeight() const
{
	return img_.size().height;
}

int FileCameraSource::GetFrameChannels() const
{
	// image is converted to 4 channels to match textures in unity
	return 4;
}

float FileCameraSource::GetFocalLength() const
{
	return 1.f;
}

void FileCameraSource::SetProperties(const nlohmann::json &json_config)
{
}

nlohmann::json FileCameraSource::GetProperties() const
{
	return nlohmann::json();
}
