#pragma once

#include <atomic>
#include <condition_variable>
#include <memory>
#include <mutex>
#include <thread>
#include <vector>

#include "cameras/CameraSourceInterface.h"
#include "frames/FrameData.h"
#include "frames/FrameSize.h"
#include "utils/UIDGenerator.h"
#include "utils/Event.h"

namespace ImageProcessing
{
	class ActiveCamera
	{
		// singleton
	public:
		static ActiveCamera * Instance()
		{
			static ActiveCamera *instance = new ActiveCamera();
			return instance;
		}

	private:
		std::mutex cam_source_mutex_;
		std::mutex frame_data_mutex_;
		std::mutex frame_id_mutex_;

		std::thread thread_;
		bool is_running_;

		std::atomic<int> frame_counter_;
		std::condition_variable frame_notifier_;
		std::shared_ptr<CameraSourceInterface> camera_source_;
		FrameSize current_framesize_;

		std::unique_ptr<unsigned char[]> framebuffer_left_, framebuffer_right_;

	private:
		ActiveCamera();
		~ActiveCamera();

		void Run();

	public:
		// Camera should be open (or will be opened) so that frame size can be determined
		void SetActiveSource(const std::shared_ptr<CameraSourceInterface> &cam);
		std::shared_ptr<CameraSourceInterface> GetSource();

		void WaitForNewFrame(int current_frame_id);
		int WriteFrame(FrameData &frame);

		void FetchNewFrame();

		Event<const FrameSize&> on_framesize_changed;
		FrameSize GetFrameSize() const;

		void Start();
		void Stop();
	};
}
