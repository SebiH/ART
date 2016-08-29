#include "outputs/JsonOutput.h"

#include "frames/JsonFrameData.h"
#include "utils/Logger.h"

using namespace ImageProcessing;

JsonOutput::JsonOutput(JsonCallback &callback)
	: callback_(callback)
{

}


JsonOutput::~JsonOutput()
{

}


void JsonOutput::RegisterResult(const FrameData &frame)
{
	// write result immediately, don't wait
	Write(frame);
}


void JsonOutput::WriteResult()
{
	// result is written immediately upon receiving
}

void JsonOutput::Write(const FrameData &frame) noexcept
{
	if (auto json_frame = dynamic_cast<const JsonFrameData *>(&frame))
	{
		callback_(json_frame->json.c_str());
	}
	else
	{
		DebugLog("Could not trigger callback: Provided frame does not have JSON!");
	}
}


