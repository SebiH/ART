#pragma once

#include "frames/FrameData.h"
#include <string>

namespace ImageProcessing
{
	class JsonFrameData : public FrameData
	{
	public: 
		std::string json;

		virtual ~JsonFrameData() { };
	};
}
