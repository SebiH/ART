#include <memory>
#include <Unity/IUnityInterface.h>

#include "processors/Processor.h"
#include "pipelines/PipelineManager.h"


extern "C" UNITY_INTERFACE_EXPORT int AddArToolkitProcessor(const int pipeline_id)
{
	// TODO.
	return pipeline_id;
}
