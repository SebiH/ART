#include "ThreadedPipeline.h"

#include <algorithm>

#include "cameras/ActiveCamera.h"
#include "utils/UIDGenerator.h"
#include "utils/Logger.h"

using namespace ImageProcessing;

ThreadedPipeline::ThreadedPipeline()
	: id_(UIDGenerator::Instance()->GetUID())
{
	framesize_changed_handler_ = [&](const FrameSize &new_size) { ResizeBuffers(new_size); };
	ActiveCamera::Instance()->on_framesize_changed += framesize_changed_handler_;

	ResizeBuffers(ActiveCamera::Instance()->GetFrameSize());
}


ThreadedPipeline::~ThreadedPipeline()
{
	ActiveCamera::Instance()->on_framesize_changed -= framesize_changed_handler_;
	Stop();
}



void ThreadedPipeline::ResizeBuffers(const FrameSize &new_size)
{
	auto buffer_size = new_size.BufferSize();

	{
		std::unique_lock<std::mutex> lock(buffer_mutex_);

		back_buffer_left_ = std::shared_ptr<unsigned char>(new unsigned char[buffer_size], std::default_delete<unsigned char[]>());
		back_buffer_right_ = std::shared_ptr<unsigned char>(new unsigned char[buffer_size], std::default_delete<unsigned char[]>());
		front_buffer_left_ = std::shared_ptr<unsigned char>(new unsigned char[buffer_size], std::default_delete<unsigned char[]>());
		front_buffer_right_ = std::shared_ptr<unsigned char>(new unsigned char[buffer_size], std::default_delete<unsigned char[]>());

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
	if (!is_running_)
	{
		is_running_ = true;
		thread_ = std::thread(&ThreadedPipeline::Run, this);
	}
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
		DebugLog(std::string("Could not stop pipeline thread: ") + e.what());
	}
}

void ThreadedPipeline::Run()
{
	auto camera = ActiveCamera::Instance();
	int current_frame_id = -1;

	while (is_running_)
	{
		// wait for new frame - Buffer resize events should happen before creating a frame with inappropriately sized buffers
		try
		{
			camera->WaitForNewFrame(current_frame_id);
		}
		catch (const std::exception &e)
		{
			DebugLog(std::string("Failed waiting for new frame: ") + e.what());
			continue;
		}

		// create frame with back buffer
		// TODO: (micro optimization) create both frames outside of loop, reuse memory,
		//		  alternate frames instead of buffers. Might need some consideration if
		//        Processors return/alter framedata?

		auto frame = CreateFrame();
		try
		{
			camera->WriteFrame(frame, current_framesize_);
		}
		catch (const std::exception &e)
		{
			DebugLog(std::string("Unable to write: ") + e.what());
			continue;
		}

		{
			std::unique_lock<std::mutex> lock(list_mutex_);
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
		}

		SwitchBuffers();
	}
}






void ThreadedPipeline::AddProcessor(std::shared_ptr<Processor> &processor)
{
	std::unique_lock<std::mutex> lock(list_mutex_);
	processors_.push_back(processor);
}


std::shared_ptr<Processor> ThreadedPipeline::GetProcessor(UID processor_id)
{
	std::unique_lock<std::mutex> lock(list_mutex_);
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
	std::unique_lock<std::mutex> lock(list_mutex_);
	processors_.erase(std::remove_if(processors_.begin(), processors_.end(), [processor_id](const std::shared_ptr<Processor> &processor) {
		return processor->Id() == processor_id;
	}), processors_.end());
}




void ThreadedPipeline::AddOutput(std::shared_ptr<Output> &output)
{
	std::unique_lock<std::mutex> lock(list_mutex_);
	outputs_.push_back(output);
}


std::shared_ptr<Output> ThreadedPipeline::GetOutput(UID output_id)
{
	std::unique_lock<std::mutex> lock(list_mutex_);
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
	std::unique_lock<std::mutex> lock(list_mutex_);
	outputs_.erase(std::remove_if(outputs_.begin(), outputs_.end(), [output_id](const std::shared_ptr<Output> &output) {
		return output->Id() == output_id;
	}), outputs_.end());
}


void ThreadedPipeline::FlushOutputs()
{
	std::unique_lock<std::mutex> lock(list_mutex_);
	for (auto output : outputs_)
	{
		output->WriteResult();
	}
}
