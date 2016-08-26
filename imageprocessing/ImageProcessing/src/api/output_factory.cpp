#include <string>
#include <memory>
#include <Unity/IUnityInterface.h>

#include "outputs/Output.h"
#include "outputs/OpenCvOutput.h"
#include "outputs/UnityTextureOutput.h"
#include "pipelines/PipelineManager.h"
#include "utils/Logger.h"

using namespace ImageProcessing;

extern "C" UNITY_INTERFACE_EXPORT int AddOpenCVOutput(int pipeline_id, std::string windowname)
{
	try
	{
		auto pipeline = PipelineManager::Instance()->GetPipeline(pipeline_id);
		std::shared_ptr<Output> opencv_output = std::make_shared<OpenCvOutput>(windowname);
		pipeline->AddOutput(opencv_output);
		return opencv_output->Id();
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to add opencv output: ") + e.what());
	}

	return -1;
}

extern "C" UNITY_INTERFACE_EXPORT int RegisterUnityPointer(int pipeline_id, int eye, unsigned char *texture_ptr)
{
	try
	{
		auto pipeline = PipelineManager::Instance()->GetPipeline(pipeline_id);

		auto unity_eye = static_cast<UnityTextureOutput::Eye>(eye);
		std::shared_ptr<Output> unity_output = std::make_shared<UnityTextureOutput>(unity_eye, texture_ptr);
		pipeline->AddOutput(unity_output);
		return unity_output->Id();
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to add unity output: ") + e.what());
	}

	return -1;
}

extern "C" UNITY_INTERFACE_EXPORT void RemoveOutput(int pipeline_id, int output_id)
{
	try
	{
		auto pipeline = PipelineManager::Instance()->GetPipeline(pipeline_id);
		pipeline->RemoveOutput(output_id);
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to remove output: ") + e.what());
	}
}
