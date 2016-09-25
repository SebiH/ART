#include "ArucoProcessor.h"

#include <aruco/aruco.h>

using namespace ImageProcessing;

ArucoProcessor::ArucoProcessor()
{
}

ArucoProcessor::~ArucoProcessor()
{

}

std::shared_ptr<const FrameData> ArucoProcessor::Process(const std::shared_ptr<const FrameData>& frame)
{
	return std::shared_ptr<const FrameData>();
}

nlohmann::json ArucoProcessor::GetProperties()
{
	return nlohmann::json();
}

void ArucoProcessor::SetProperties(const nlohmann::json &config)
{
}
