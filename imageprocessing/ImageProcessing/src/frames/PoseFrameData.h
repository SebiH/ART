#pragma once

#include "frames/FrameData.h"

namespace ImageProcessing
{
	class PoseFrameData : public FrameData
	{
	public: 
		double *pose;

		PoseFrameData();
		virtual ~PoseFrameData();
	};
}
