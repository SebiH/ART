#pragma once

#include <memory>
#include <mutex>
#include <thread>
#include <vector>

#include "frames/FrameData.h"
#include "frames/FrameSize.h"
#include "outputs/Output.h"
#include "processors/Processor.h"
#include "utils/UID.h"
#include "utils/UIDGenerator.h"

namespace ImageProcessing
{
	class ThreadedPipeline
	{
	private:
		const UID id_;
		std::mutex list_mutex_;
		std::vector<std::shared_ptr<Processor>> processors_;
		std::vector<std::shared_ptr<Output>> outputs_;

		std::thread thread_;
		bool is_running_ = false;

		std::mutex buffer_mutex_;
		std::shared_ptr<unsigned char> back_buffer_left_;
		std::shared_ptr<unsigned char> back_buffer_right_;
		std::shared_ptr<unsigned char> front_buffer_left_;
		std::shared_ptr<unsigned char> front_buffer_right_;

		UIDGenerator frame_uid_generator;
		FrameSize current_framesize_;


	public:
		ThreadedPipeline();
		virtual ~ThreadedPipeline();

		int Id() const { return id_; };

		void Start();
		void Stop();

		void AddProcessor(std::shared_ptr<Processor> &processor);
		std::shared_ptr<Processor> GetProcessor(UID processor_id);
		void RemoveProcessor(UID processor_id);

		void AddOutput(std::shared_ptr<Output> &output);
		std::shared_ptr<Output> GetOutput(UID output_id);
		void RemoveOutput(UID output_id);

		void FlushOutputs();

	private:
		void Run();

		// TODO: should be Event<..>::EventHandler, but throws errors?
		std::function<void(const FrameSize &)> framesize_changed_handler_;
		void ResizeBuffers(const FrameSize &new_size);
		FrameData CreateFrame();
		void SwitchBuffers();
	};
}
