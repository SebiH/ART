#include <Unity/IUnityInterface.h>
#include "PipelineManager.h"


extern "C" UNITY_INTERFACE_EXPORT int CreatePipeline()
{
	auto pipeline = ImageProcessing::PipelineManager::Instance()->CreatePipeline();
	pipeline->Start();
	return pipeline->Id();
}

extern "C" UNITY_INTERFACE_EXPORT void DestroyPipeline(int uid)
{
	auto pipeline = ImageProcessing::PipelineManager::Instance()->GetPipeline(uid);

	if (pipeline.get())
	{
		pipeline->Stop();
	}
}
