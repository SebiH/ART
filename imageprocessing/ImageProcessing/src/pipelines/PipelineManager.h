#pragma once

#include <memory>
#include <vector>
#include "pipelines/ThreadedPipeline.h"

namespace ImageProcessing
{
	class PipelineManager
	{
		// singleton
	public:
		static PipelineManager * Instance()
		{
			static PipelineManager *instance = new PipelineManager();
			return instance;
		}

	private:
		PipelineManager();
		virtual ~PipelineManager();

		std::vector<std::shared_ptr<ThreadedPipeline>> pipelines_;

	public:
		std::shared_ptr<ThreadedPipeline> CreatePipeline();
		std::shared_ptr<ThreadedPipeline> GetPipeline(UID pipeline_id);
		void RemovePipeline(UID pipeline_id);
	};
}
