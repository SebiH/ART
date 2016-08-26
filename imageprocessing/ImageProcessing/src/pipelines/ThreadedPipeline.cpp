#include "ThreadedPipeline.h"

#include <algorithm>

#include "cameras/ActiveCamera.h"
#include "utils/UIDGenerator.h"
#include "utils/Logger.h"

using namespace ImageProcessing;

ThreadedPipeline::ThreadedPipeline()
	: id_(UIDGenerator::Instance()->GetUID()),
	  buffer_mutex_()
{
	framesize_changed_handler_ = [&](const FrameSize &new_size) { ResizeBuffers(new_size); };
	ActiveCamera::Instance()->on_framesize_changed += framesize_changed_handler_;

	ResizeBuffers(ActiveCamera::Instance()->GetFrameSize());
}


ThreadedPipeline::~ThreadedPipeline()
{
	ActiveCamera::Instance()->on_framesize_changed -= framesize_changed_handler_;
}



void ThreadedPipeline::ResizeBuffers(const FrameSize &new_size)
{
	auto buffer_size = new_size.GetBufferSize();

	{
		std::unique_lock<std::mutex> lock(buffer_mutex_);

		back_buffer_left_ = std::make_shared<unsigned char[]>(buffer_size);
		back_buffer_right_ = std::make_shared<unsigned char[]>(buffer_size);
		front_buffer_left_ = std::make_shared<unsigned char[]>(buffer_size);
		front_buffer_right_ = std::make_shared<unsigned char[]>(buffer_size);

		current_framesize_ = new_size;
	}
}

FrameData ThreadedPipeline::CreateFrame()
{
	auto frame_id = frame_uid_generator.GetUID();
	std::unique_lock<std::mutex> lock(buffer_mutex_);
	return FrameData(frame_id, back_buffer_left_, back_buffer_right_, current_framesize_);
}

void ThreadedPipeline::SwitchBuffers()
{
	std::unique_lock<std::mutex> lock(buffer_mutex_);

	auto tmp_left = back_buffer_left_;
	auto tmp_right = back_buffer_right_;
	
	back_buffer_left_ = front_buffer_left_;
	back_buffer_right_ = front_buffer_right_;

	front_buffer_left_ = tmp_left;
	front_buffer_right_ = tmp_right;
}



void ThreadedPipeline::Start()
{
	is_running_ = true;
	thread_ = std::thread(&ThreadedPipeline::Run, this);
}


void ThreadedPipeline::Stop()
{
	try
	{
		is_running_ = false;
		thread_.join();
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Could not stop thread: ") + e.what());
	}
}

void ThreadedPipeline::Run()
{
	auto camera = ActiveCamera::Instance();
	int current_frame_id = -1;

	while (is_running_)
	{
		// create frame with back buffer
		// TODO: (micro optimization) create both frames outside of loop, reuse memory,
		//		  alternate frames instead of buffers. Might need some consideration if
		//        Processors return/alter framedata?
		auto frame = CreateFrame();

		// Grab new frame
		camera->WaitForNewFrame(current_frame_id);
		camera->WriteFrame(frame);

		// pass frame into all processing modules
		for (auto processor : processors_)
		{
			processor->Process(frame);
		}

		// register backbuffer as result in output module
		for (auto output : outputs_)
		{
			output->RegisterResult(frame);
		}

		SwitchBuffers();
	}
}






void ThreadedPipeline::AddProcessor(std::shared_ptr<Processor> &processor)
{
	processors_.push_back(processor);
}


std::shared_ptr<Processor> ThreadedPipeline::GetProcessor(UID processor_id)
{
	for (auto processor : processors_)
	{
		if (processor->Id() == processor_id)
		{
			return processor;
		}
	}

	throw std::exception("Unknown processor id");
}


void ThreadedPipeline::RemoveProcessor(UID processor_id)
{
	processors_.erase(std::remove_if(processors_.begin(), processors_.end(), [processor_id](const Processor &processor) {
		return processor.Id() == processor_id;
	}), processors_.end());
}




void ThreadedPipeline::AddOutput(std::shared_ptr<Output> &output)
{
	outputs_.push_back(output);
}


std::shared_ptr<Output> ThreadedPipeline::GetOutput(UID output_id)
{
	for (auto output : outputs_)
	{
		if (output->Id() == output_id)
		{
			return output;
		}
	}

	throw std::exception("Unknown output id");
}


void ThreadedPipeline::RemoveOutput(UID output_id)
{
	outputs_.erase(std::remove_if(outputs_.begin(), outputs_.end(), [output_id](const Output &output) {
		return output.Id() == output_id;
	}), outputs_.end());
}
