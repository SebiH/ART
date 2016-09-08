#include "cameras/ActiveCamera.h"

#include <algorithm>
#include <chrono>
#include "utils/Logger.h"

using namespace ImageProcessing;

ActiveCamera::ActiveCamera()
	: frame_counter_(ATOMIC_VAR_INIT(-1))
{
}

ActiveCamera::~ActiveCamera()
{
	Stop();
}


void ActiveCamera::Start()
{
	if (!is_running_)
	{
		is_running_ = true;
		thread_ = std::thread(&ActiveCamera::Run, this);
	}
}


void ActiveCamera::Stop()
{
	try
	{
		is_running_ = false;
		auto cam_src = GetSource();
		if (cam_src && cam_src->IsOpen())
		{
			cam_src->Close();
		}

		frame_notifier_.notify_all();
		thread_.join();
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Could not stop camera thread: ") + e.what());
	}
}


void ActiveCamera::Run()
{
	while (is_running_)
	{
		FetchNewFrame();
	}
}



void ActiveCamera::FetchNewFrame()
{
	// Use copy (instead of member) to avoid lock over whole FetchFrame operation
	auto cam_src = GetSource();

#if MEASURE_FPS
	static auto s_last = std::chrono::high_resolution_clock::now();
	static int s_fps_counter = 0;
	auto duration = std::chrono::high_resolution_clock::now() - s_last;

	if (std::chrono::duration_cast<std::chrono::seconds>(duration).count() >= 1)
	{
		DebugLog(std::string("FPS ") + std::to_string(s_fps_counter));
		s_fps_counter = 0;
		s_last = std::chrono::high_resolution_clock::now();
	}
#endif

	if (!cam_src || !cam_src->IsOpen())
	{
		// TODO: not optimal..
		std::this_thread::sleep_for(std::chrono::milliseconds(100));
		return;
	}

	try
	{
		cam_src->PrepareNextFrame();

#if MEASURE_FPS
		s_fps_counter++;
#endif
		

		{
			std::lock_guard<std::mutex> frame_lock(mutex_);

			if (cam_src != camera_source_)
			{
				throw std::exception("Camera source has changed!");
			}

			cam_src->GrabFrame(framebuffer_left_.get(), framebuffer_right_.get());
		}

		frame_counter_++;
		frame_notifier_.notify_all();
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Could not grab new frame: ") + e.what());
	}
}



void ActiveCamera::SetActiveSource(const std::shared_ptr<CameraSourceInterface> &cam)
{
	std::lock_guard<std::mutex> lock(mutex_);
	camera_source_ = cam;

	if (cam)
	{
		if (!cam->IsOpen())
		{
			cam->Open();
		}

		current_framesize_ = FrameSize(cam->GetFrameWidth(), cam->GetFrameHeight(), cam->GetFrameChannels());
		on_framesize_changed(current_framesize_);

		auto buffer_size = current_framesize_.BufferSize();
		framebuffer_left_ = std::make_unique<unsigned char[]>(buffer_size);
		framebuffer_right_ = std::make_unique<unsigned char[]>(buffer_size);
	}

}

std::shared_ptr<CameraSourceInterface> ActiveCamera::GetSource()
{
	std::lock_guard<std::mutex> lock(mutex_);
	return camera_source_;
}


void ActiveCamera::WaitForNewFrame(int consumer_frame_id)
{
	std::unique_lock<std::mutex> lock(frame_id_mutex_);
	frame_notifier_.wait(lock, [&]() { return (frame_counter_ > consumer_frame_id) || !(is_running_); });

	if (!is_running_)
	{
		throw std::exception("Thread is no longer active");
	}
}

int ActiveCamera::WriteFrame(const FrameData *frame)
{
	auto current_framecounter = frame_counter_.load();

	{
		std::lock_guard<std::mutex> lock(mutex_);

		if (frame->size != current_framesize_)
		{
			// TODO: workaround for nasty threading issues
			throw std::exception("Unable to write frame; mismatching frame sizes");
		}

		auto buffer_size = frame->size.BufferSize();

		std::memcpy(frame->buffer_left.get(), framebuffer_left_.get(), buffer_size);
		std::memcpy(frame->buffer_right.get(), framebuffer_right_.get(), buffer_size);
	}

	return current_framecounter;
}


FrameSize ActiveCamera::GetFrameSize() const
{
	return current_framesize_;
}
