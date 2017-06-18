#include "ThreadedPipeline.h"

#include <algorithm>

#include "debugging/MeasurePerformance.h"
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

	list_lock_ = new SRWLOCK();
	InitializeSRWLock(list_lock_);
}


ThreadedPipeline::~ThreadedPipeline()
{
	ActiveCamera::Instance()->on_framesize_changed -= framesize_changed_handler_;
	Stop();
	delete list_lock_;
}



void ThreadedPipeline::ResizeBuffers(const FrameSize &new_size)
{
	auto buffer_size = new_size.BufferSize();

	{
		std::lock_guard<std::mutex> lock(buffer_mutex_);

		back_buffer_left_ = std::shared_ptr<unsigned char>(new unsigned char[buffer_size], std::default_delete<unsigned char[]>());
		back_buffer_right_ = std::shared_ptr<unsigned char>(new unsigned char[buffer_size], std::default_delete<unsigned char[]>());
		front_buffer_left_ = std::shared_ptr<unsigned char>(new unsigned char[buffer_size], std::default_delete<unsigned char[]>());
		front_buffer_right_ = std::shared_ptr<unsigned char>(new unsigned char[buffer_size], std::default_delete<unsigned char[]>());

		current_framesize_ = new_size;
	}
}

std::shared_ptr<const FrameData> ThreadedPipeline::CreateFrame()
{
	auto frame_id = frame_uid_generator.GetUID();
	std::lock_guard<std::mutex> lock(buffer_mutex_);
	return std::make_shared<FrameData>(frame_id, back_buffer_left_, back_buffer_right_, current_framesize_);
}

void ThreadedPipeline::SwitchBuffers()
{
	std::lock_guard<std::mutex> lock(buffer_mutex_);

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

		PERF_MEASURE(start)
		auto frame = CreateFrame();
		try
		{
			current_frame_id = camera->WriteFrame(frame.get());
		}
		catch (const std::exception &e)
		{
			DebugLog(std::string("Unable to write: ") + e.what());
			continue;
		}

		{
			// TODO: put into try/catch, or write class similar to std::lock_guard??
			AcquireSRWLockShared(list_lock_);

			try
			{
				// pass frame into all processing modules
				for (const auto &processor : processors_)
				{
					frame = processor->Process(frame);
				}

				// register backbuffer as result in output module
				for (const auto &output : outputs_)
				{
					output->RegisterResult(frame);
				}
			}
			catch (const std::exception &e)
			{
				DebugLog(e.what());
			}

			PERF_MEASURE(end)
			ReleaseSRWLockShared(list_lock_);
			//PERF_OUTPUT("pipeline: ", start, end)
		}

		SwitchBuffers();
	}
}






void ThreadedPipeline::AddProcessor(std::shared_ptr<Processor> &processor)
{
	AcquireSRWLockExclusive(list_lock_);
	processors_.push_back(processor);
	ReleaseSRWLockExclusive(list_lock_);
}


std::shared_ptr<Processor> ThreadedPipeline::GetProcessor(UID processor_id)
{
	AcquireSRWLockShared(list_lock_);
	for (const auto &processor : processors_)
	{
		if (processor->Id() == processor_id)
		{
			ReleaseSRWLockShared(list_lock_);
			return processor;
		}
	}

	ReleaseSRWLockShared(list_lock_);
	throw std::exception("Unknown processor id");
}


void ThreadedPipeline::RemoveProcessor(UID processor_id)
{
	AcquireSRWLockExclusive(list_lock_);
	processors_.erase(std::remove_if(processors_.begin(), processors_.end(), [processor_id](const std::shared_ptr<Processor> &processor) {
		return processor->Id() == processor_id;
	}), processors_.end());
	ReleaseSRWLockExclusive(list_lock_);
}




void ThreadedPipeline::AddOutput(const std::shared_ptr<Output> &output)
{
	AcquireSRWLockExclusive(list_lock_);
	outputs_.push_back(output);
	ReleaseSRWLockExclusive(list_lock_);
}


std::shared_ptr<Output> ThreadedPipeline::GetOutput(UID output_id)
{
	AcquireSRWLockShared(list_lock_);
	for (const auto &output : outputs_)
	{
		if (output->Id() == output_id)
		{
			ReleaseSRWLockShared(list_lock_);
			return output;
		}
	}

	ReleaseSRWLockShared(list_lock_);
	throw std::exception("Unknown output id");
}


void ThreadedPipeline::RemoveOutput(UID output_id)
{
	AcquireSRWLockExclusive(list_lock_);
	outputs_.erase(std::remove_if(outputs_.begin(), outputs_.end(), [output_id](const std::shared_ptr<Output> &output) {
		return output->Id() == output_id;
	}), outputs_.end());
	ReleaseSRWLockExclusive(list_lock_);
}


void ThreadedPipeline::FlushOutputs()
{
	AcquireSRWLockShared(list_lock_);
	for (const auto &output : outputs_)
	{
		output->WriteResult();
	}
	ReleaseSRWLockShared(list_lock_);
}
