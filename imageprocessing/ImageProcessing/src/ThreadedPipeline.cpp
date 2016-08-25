#include "ThreadedPipeline.h"

#include "utils/UIDGenerator.h"
#include "utils/Logger.h"

using namespace ImageProcessing;

ThreadedPipeline::ThreadedPipeline()
	: id_(UIDGenerator::Instance()->GetUID())
{

}


ThreadedPipeline::~ThreadedPipeline()
{

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
		thread_.join();
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Could not stop thread: ") + e.what());
	}
}

void ThreadedPipeline::Run()
{
	while (is_running_)
	{

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

}
