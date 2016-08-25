#include "cameras/ActiveCamera.h"

#include <algorithm>
#include "utils/Logger.h"

using namespace ImageProcessing;

ActiveCamera::ActiveCamera()
	: cam_source_mutex_(),
	  frame_data_mutex_(),
	  frame_id_mutex_(),
	  frame_notifier_()
{

}

ActiveCamera::~ActiveCamera()
{
}


void ActiveCamera::ResizeBuffers(const FrameMetaData &new_data)
{
	auto buffer_size = new_data.GetBufferSize();
	frame_data_left_ = std::make_unique<unsigned char[]>(buffer_size);
	frame_data_right_ = std::make_unique<unsigned char[]>(buffer_size);
}

void ActiveCamera::FetchFrame()
{
	// Use copy (instead of member) to avoid lock over whole FetchFrame operation
	auto cam_src = GetSource();

	if (cam_src.get() == nullptr || !cam_src->IsOpen())
	{
		return;
	}

	try
	{
		cam_src->GrabFrame(frame_data_left_.get(), frame_data_right_.get());
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Could not grab new frame: ") + e.what());
	}
}



void ActiveCamera::SetSource(const std::shared_ptr<CameraSourceInterface> &cam)
{
	std::unique_lock<std::mutex> lock(cam_source_mutex_);
	camera_source_ = cam;

	if (cam.get() != nullptr)
	{
		auto new_frame_data = cam->GetFrameMetaData();

		if (current_frame_metadata_ != new_frame_data)
		{
			// fire OnSourceChanged events, resize buffers, re-process FrameMetaData
			current_frame_metadata_ = new_frame_data;
			ResizeBuffers(new_frame_data);
		}
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

int ActiveCamera::WriteFrame(unsigned char *left_buffer, unsigned char *right_buffer)
{
	std::unique_lock<std::mutex> lock(frame_data_mutex_);

	if (left_buffer != nullptr)
	{
		std::memcpy(left_buffer, frame_data_left_.get(), 0);
	}

	if (right_buffer != nullptr)
	{
		std::memcpy(right_buffer, frame_data_right_.get(), 0);
	}
}


FrameMetaData ActiveCamera::GetCurrentFrameData()
{
	std::unique_lock<std::mutex> lock(frame_data_mutex_);
	return current_frame_metadata_;
}
