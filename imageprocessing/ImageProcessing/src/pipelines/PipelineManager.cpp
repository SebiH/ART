#include "pipelines/PipelineManager.h"

#include <algorithm>

using namespace ImageProcessing;

PipelineManager::PipelineManager()
{

}

PipelineManager::~PipelineManager()
{

}



std::shared_ptr<ThreadedPipeline> PipelineManager::CreatePipeline()
{
	auto pipeline = std::make_shared<ThreadedPipeline>();
	pipelines_.push_back(pipeline);
	return pipeline;
}


std::shared_ptr<ThreadedPipeline> PipelineManager::GetPipeline(UID pipeline_id)
{
	for (auto pipeline : pipelines_)
	{
		if (pipeline->Id() == pipeline_id)
		{
			return pipeline;
		}
	}

	throw std::exception("Unknown pipeline id");
}


void PipelineManager::RemovePipeline(UID pipeline_id)
{
	pipelines_.erase(std::remove_if(pipelines_.begin(), pipelines_.end(), [pipeline_id](const std::shared_ptr<ThreadedPipeline> &pipeline) {
		return pipeline->Id() == pipeline_id;
	}), pipelines_.end());
}
