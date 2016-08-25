#pragma once

#include <atomic>
#include <condition_variable>
#include <memory>
#include <mutex>
#include <vector>

#include "cameras/CameraSourceInterface.h"
#include "cameras/FrameMetaData.h"
#include "utils/Event.h"
#include "utils/UIDGenerator.h"

namespace ImageProcessing
{
	class ActiveCamera
	{
		// singleton
	private:
		static ActiveCamera* s_instance_;

	public:
		static ActiveCamera * Instance()
		{
			if (!s_instance_)
			{
				s_instance_ = new ActiveCamera();
			}
			return s_instance_;
		}

	private:
		std::mutex cam_source_mutex_;
		std::mutex frame_data_mutex_;
		std::mutex frame_id_mutex_;

		std::condition_variable frame_notifier_;
		std::shared_ptr<CameraSourceInterface> camera_source_;

		std::unique_ptr<UIDGenerator> frame_uid_generator;
		std::unique_ptr<unsigned char[]> frame_data_left_, frame_data_right_;
		FrameMetaData current_frame_metadata_;

	public:
		Event<const FrameMetaData&> OnImageMetaDataChanged;

	private:
		ActiveCamera();
		~ActiveCamera();

	private:
		void ResizeBuffers(const FrameMetaData &new_data);
		void FetchFrame();

	public:
		void SetSource(const std::shared_ptr<CameraSourceInterface> &cam);
		std::shared_ptr<CameraSourceInterface> GetSource();

		void WaitForNewFrame(int current_frame_id);
		int WriteFrame(unsigned char *left_buffer, unsigned char *right_buffer);
		FrameMetaData GetCurrentFrameData();
	};
}
