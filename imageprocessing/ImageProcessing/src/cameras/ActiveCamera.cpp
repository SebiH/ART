#include "cameras/ActiveCamera.h"

#include <algorithm>
#include "utils/Logger.h"

using namespace ImageProcessing;

ActiveCamera::ActiveCamera()
	: cam_source_mutex_(),
	  frame_data_mutex_(),
	  frame_id_mutex_(),
	  frame_notifier_(),
      frame_counter_(ATOMIC_VAR_INIT(1))
{
}

ActiveCamera::~ActiveCamera()
{
}


void ActiveCamera::FetchNewFrame()
{
	// Use copy (instead of member) to avoid lock over whole FetchFrame operation
	auto cam_src = GetSource();

	if (cam_src.get() == nullptr || !cam_src->IsOpen())
	{
		return;
	}

	try
	{
		cam_src->GrabFrame(framebuffer_left_.get(), framebuffer_right_.get());
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Could not grab new frame: ") + e.what());
	}
}



void ActiveCamera::SetActiveSource(const std::shared_ptr<CameraSourceInterface> &cam)
{
	{
		std::unique_lock<std::mutex> lock(cam_source_mutex_);
		camera_source_ = cam;
	}

	if (cam.get() != nullptr)
	{
		if (!cam->IsOpen())
		{
			cam->Open();
		}

		{
			std::unique_lock<std::mutex> lock(frame_data_mutex_);
			current_framesize_ = FrameSize(cam->GetFrameWidth(), cam->GetFrameHeight(), cam->GetFrameChannels());
		}

		auto buffer_size = current_framesize_.GetBufferSize();
		framebuffer_left_ = std::make_unique<unsigned char[]>(buffer_size);
		framebuffer_right_ = std::make_unique<unsigned char[]>(buffer_size);
	}

}

std::shared_ptr<CameraSourceInterface> ActiveCamera::GetSource()
{
	std::unique_lock<std::mutex> lock(cam_source_mutex_);
	return camera_source_;
}


void ActiveCamera::WaitForNewFrame(int consumer_frame_id)
{
	std::unique_lock<std::mutex> lock(frame_id_mutex_);
	frame_notifier_.wait(lock, [&]() { return frame_counter_ > consumer_frame_id; });
}

int ActiveCamera::WriteFrame(FrameData &frame)
{
	auto current_framecounter = frame_counter_.load();

	{
		std::unique_lock<std::mutex> lock(frame_data_mutex_);

		frame.size = current_framesize_;
		auto buffer_size = frame.size.GetBufferSize();

		std::memcpy(frame.buffer_left.get(), framebuffer_left_.get(), buffer_size);
		std::memcpy(frame.buffer_right.get(), framebuffer_right_.get(), buffer_size);
	}

	return current_framecounter;
}


FrameSize ActiveCamera::GetFrameSize() const
{
	return current_framesize_;
}
