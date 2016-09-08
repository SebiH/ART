#include "cameras/OpenCVCameraSource.h"

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
}

void OpenCVCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
	cv::Mat frame;
	camera_->retrieve(frame);

	if (frame.channels() == 4)
	{
		// convert back to 3-channel BGR
		cv::cvtColor(frame, frame, CV_BGRA2BGR);
	}
	else if (frame.channels() != GetFrameChannels())
	{
		throw std::exception("Camera provided unexpected amount of channels");
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
	return 3;
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
