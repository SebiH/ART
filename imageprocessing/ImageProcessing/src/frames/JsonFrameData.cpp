#include "JsonFrameData.h"

#include <json/json.hpp>

using namespace ImageProcessing;

JsonFrameData::JsonFrameData(const FrameData * parent, nlohmann::json json_data)
	: FrameData(*parent),
	  json(json_data.dump())
{
}
