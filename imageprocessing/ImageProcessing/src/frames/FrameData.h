#pragma once

#include <memory>
#include "utils/UID.h"
#include "frames/FrameSize.h"

namespace ImageProcessing
{
	class FrameData
	{
	public:
		UID id;
		std::shared_ptr<unsigned char[]> buffer_left;
		std::shared_ptr<unsigned char[]> buffer_right;
		FrameSize size;
		bool is_valid;

		FrameData() : is_valid(false) {}

		FrameData(const UID id, const std::shared_ptr<unsigned char[]> &buffer_left, const std::shared_ptr<unsigned char[]> &buffer_right, const FrameSize &size)
			: id(id),
			  buffer_left(buffer_left),
			  buffer_right(buffer_right),
			  size(size),
			  is_valid(true) { }

		virtual ~FrameData() { };
	};
}
