#pragma once

#include <memory>
#include <thread>
#include <vector>

#include "outputs/Output.h"
#include "outputs/OutputType.h"
#include "processors/Processor.h"
#include "processors/ProcessorType.h"
#include "utils/UID.h"

namespace ImageProcessing
{
	class ThreadedPipeline
	{
	private:
		const UID id_;
		std::vector<std::shared_ptr<Processor>> processors_;
		std::vector<std::shared_ptr<Output>> outputs_;

		bool is_running_;
		std::thread thread_;

		std::shared_ptr<unsigned char[]> back_buffer_left_;
		std::shared_ptr<unsigned char[]> back_buffer_right_;
		std::shared_ptr<unsigned char[]> front_buffer_left_;
		std::shared_ptr<unsigned char[]> front_buffer_right_;


	public:
		ThreadedPipeline();
		virtual ~ThreadedPipeline();

		int Id() const { return id_; };

		void OnFrameSizeChanged(const FrameSize &new_size);

		void Start();
		void Stop();

		void AddProcessor(std::shared_ptr<Processor> &processor);
		std::shared_ptr<Processor> GetProcessor(UID processor_id);
		void RemoveProcessor(UID processor_id);

		void AddOutput(std::shared_ptr<Output> &output);
		std::shared_ptr<Output> GetOutput(UID output_id);
		void RemoveOutput(UID output_id);

	private:
		void Run();
	};
}
