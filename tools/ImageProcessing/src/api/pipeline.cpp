#include <string>
#include <Unity/IUnityInterface.h>
#include "pipelines/PipelineManager.h"
#include "utils/Logger.h"


extern "C" UNITY_INTERFACE_EXPORT int CreatePipeline()
{
	auto pipeline = ImageProcessing::PipelineManager::Instance()->CreatePipeline();
	pipeline->Start();
	return pipeline->Id();
}

extern "C" UNITY_INTERFACE_EXPORT void RemovePipeline(const int uid)
{
	try
	{
		auto pipeline = ImageProcessing::PipelineManager::Instance()->GetPipeline(uid);
		pipeline->Stop();
		ImageProcessing::PipelineManager::Instance()->RemovePipeline(pipeline->Id());
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Could not remove pipeline: ") + e.what());
	}
}
