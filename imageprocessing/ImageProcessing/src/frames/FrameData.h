#pragma once

#include <memory>
#include "utils/UID.h"
#include "frames/FrameSize.h"

namespace ImageProcessing
{
	class FrameData
	{
	public:
		const UID id;
		const std::shared_ptr<unsigned char[]> buffer_left;
		const std::shared_ptr<unsigned char[]> buffer_right;
		const FrameSize size;

		FrameData(UID id, std::shared_ptr<unsigned char[]> buffer_left, std::shared_ptr<unsigned char[]> buffer_right);
		virtual ~FrameData();
	};
}
