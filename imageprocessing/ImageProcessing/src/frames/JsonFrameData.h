#pragma once

#include <string>
#include <json/json.hpp>
#include "frames/FrameData.h"

namespace ImageProcessing
{
	class JsonFrameData : public FrameData
	{
	public: 
		std::string json;

		JsonFrameData(const FrameData *parent, nlohmann::json json_data);
		virtual ~JsonFrameData() { };
	};
}
