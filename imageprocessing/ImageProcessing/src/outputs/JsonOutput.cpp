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


void JsonOutput::RegisterResult(const std::shared_ptr<const FrameData> &frame)
{
	// write result immediately, don't wait
	Write(frame.get());
}


void JsonOutput::WriteResult()
{
	// result is written immediately upon receiving
}

void JsonOutput::Write(const FrameData *frame) noexcept
{
	auto json_frame = dynamic_cast<const JsonFrameData *>(frame);
	if (json_frame)
	{
		callback_(json_frame->json.c_str());
	}
}


