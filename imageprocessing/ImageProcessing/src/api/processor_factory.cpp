#include <memory>
#include <Unity/IUnityInterface.h>
#include <json/json.hpp>

#include "pipelines/PipelineManager.h"
#include "processors/Processor.h"
#include "processors/ArToolkitProcessor.h"
#include "utils/Logger.h"

using namespace ImageProcessing;

extern "C" UNITY_INTERFACE_EXPORT int AddArToolkitProcessor(const int pipeline_id, const char *json_config)
{
	try
	{
		auto pipeline = PipelineManager::Instance()->GetPipeline(pipeline_id);

		std::shared_ptr<Processor> artoolkit_processor = std::make_shared<ArToolkitProcessor>(std::string(json_config));
		pipeline->AddProcessor(artoolkit_processor);
		return artoolkit_processor->Id();
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to add ARToolkitProcessor: ") + e.what());
	}

	return -1;
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

extern "C" UNITY_INTERFACE_EXPORT const char* GetProcessorProperties(const int pipeline_id, const int processor_id)
{
	try
	{
		auto pipeline = PipelineManager::Instance()->GetPipeline(pipeline_id);
		
		if (!pipeline)
		{
			throw std::exception("No pipeline with this id");
		}

		auto processor = pipeline->GetProcessor(processor_id);
		return processor->GetProperties().dump().c_str();
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to get processor config: ") + e.what());
		return "Error";
	}

}

extern "C" UNITY_INTERFACE_EXPORT void SetProcessorProperties(const int pipeline_id, const int processor_id, const char *json_config_str)
{
	try
	{
		auto pipeline = PipelineManager::Instance()->GetPipeline(pipeline_id);
		
		if (!pipeline)
		{
			throw std::exception("No pipeline with this id");
		}

		auto processor = pipeline->GetProcessor(processor_id);
		auto config = nlohmann::json::parse(json_config_str);
		processor->SetProperties(config);
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to set processor config: ") + e.what());
	}
}
