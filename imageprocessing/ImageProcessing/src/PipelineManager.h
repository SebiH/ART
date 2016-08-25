#pragma once

#include <memory>
#include "ThreadedPipeline.h"

namespace ImageProcessing
{
	class PipelineManager
	{
		// singleton
	private:
		static PipelineManager * s_instance_;

	public:
		static PipelineManager * Instance()
		{
			if (!s_instance_)
			{
				s_instance_ = new PipelineManager();
			}

			return s_instance_;
		}

	private:
		PipelineManager();
		virtual ~PipelineManager();


	public:
		std::shared_ptr<ThreadedPipeline> CreatePipeline();
		std::shared_ptr<ThreadedPipeline> GetPipeline(UID pipeline_id);
		void RemovePipeline(UID pipeline_id);
	};
}
