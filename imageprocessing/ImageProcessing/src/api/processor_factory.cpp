#include <memory>
#include <Unity/IUnityInterface.h>

#include "processors/Processor.h"
#include "pipelines/PipelineManager.h"

using namespace ImageProcessing;

extern "C" UNITY_INTERFACE_EXPORT int AddArToolkitProcessor(const int pipeline_id)
{
	// TODO.
	return pipeline_id;
}


extern "C" UNITY_INTERFACE_EXPORT void RemoveProcessor(const int pipeline_id, const int processor_id)
{
	try
	{
		auto pipeline = PipelineManager::Instance()->GetPipeline(pipeline_id);
		
		if (!pipeline)
		{
			throw std::exception("No pipeline with this id");
		}

		pipeline->RemoveProcessor(processor_id);
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to remove processor: ") + e.what());
	}
}
