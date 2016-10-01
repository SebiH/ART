#include "cameras/OpenCVCameraSource.h"

#include <chrono>
#include <thread>
#include <opencv2/imgproc.hpp>
#include "utils/Logger.h"

using namespace ImageProcessing;

OpenCVCameraSource::OpenCVCameraSource()
{
	camera_ = std::make_unique<cv::VideoCapture>();
}

OpenCVCameraSource::~OpenCVCameraSource()
{
	Close();
}

void OpenCVCameraSource::PrepareNextFrame()
{
	camera_->grab();
	std::this_thread::sleep_for(std::chrono::milliseconds(10));
}

void OpenCVCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
	cv::Mat frame;
	camera_->retrieve(frame);

	if (frame.channels() == 3)
	{
		// convert back to 4-channel BGRA for easier unity handling
		cv::cvtColor(frame, frame, CV_BGR2BGRA);
	}
	else if (frame.channels() != GetFrameChannels())
	{
		DebugLog("Camera provided unknown amount of channels");
		//throw std::exception("Camera provided unexpected amount of channels");
	}

	auto buffer_size = GetFrameWidth() * GetFrameHeight() * GetFrameChannels();
	memcpy(left_buffer, frame.data, buffer_size);
	memcpy(right_buffer, frame.data, buffer_size);
}

void OpenCVCameraSource::Open()
{
	if (camera_ && !IsOpen())
	{
		bool open_success = camera_->open(0);

		if (!open_success)
		{
			throw std::exception("Unable to open OpenCV camera");
		}
	}
}

void OpenCVCameraSource::Close()
{
	if (camera_ && IsOpen())
	{
		camera_->release();
	}
}

bool OpenCVCameraSource::IsOpen() const
{
	return camera_->isOpened();
}

int OpenCVCameraSource::GetFrameWidth() const
{
	return static_cast<int>(camera_->get(cv::CAP_PROP_FRAME_WIDTH));
}

int OpenCVCameraSource::GetFrameHeight() const
{
	return static_cast<int>(camera_->get(cv::CAP_PROP_FRAME_HEIGHT));
}

int OpenCVCameraSource::GetFrameChannels() const
{
	//return static_cast<int>(camera_->get(cv::CAP_PROP_ ? ));
	return 4;
}

float OpenCVCameraSource::GetFocalLength() const
{
	return 0.1f;
}

nlohmann::json OpenCVCameraSource::GetProperties() const
{
	// NYI
	return nlohmann::json();
}

void OpenCVCameraSource::SetProperties(const nlohmann::json & json_config)
{
	// NYI
}
