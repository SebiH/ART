#include <memory>
#include <Unity/IUnityInterface.h>

#include "processors/Processor.h"
#include "PipelineManager.h"


extern "C" UNITY_INTERFACE_EXPORT int AddArToolkitProcessor(int pipeline_id)
{
	return 0;
}
