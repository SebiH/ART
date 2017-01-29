#include "processors/ArToolkitStereoProcessor.h"

using namespace ImageProcessing;

ArToolkitStereoProcessor::ArToolkitStereoProcessor(std::string config)
{
}

ArToolkitStereoProcessor::~ArToolkitStereoProcessor()
{
}

std::shared_ptr<const FrameData> ArToolkitStereoProcessor::Process(const std::shared_ptr<const FrameData>& frame)
{
	return std::shared_ptr<const FrameData>();
}

nlohmann::json ArToolkitStereoProcessor::GetProperties()
{
	return nlohmann::json();
}

void ArToolkitStereoProcessor::SetProperties(const nlohmann::json & config)
{
}

void ArToolkitStereoProcessor::Initialise(std::string parameter_filename, int width, int height, int depth)
{
}
