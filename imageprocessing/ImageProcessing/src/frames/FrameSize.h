#pragma once

#include <cstddef>

namespace ImageProcessing
{
	class FrameSize
	{
	public:
		int frame_width;
		int frame_height;
		int frame_depth;

	public:
		FrameSize()
			: FrameSize(0, 0, 0)
		{

		}

		FrameSize(const int frame_width, const int frame_height, const int frame_depth)
			: frame_width(frame_width),
			  frame_height(frame_height),
			  frame_depth(frame_depth)
		{ }

		virtual ~FrameSize() { };

		std::size_t GetBufferSize() const
		{
			return frame_width * frame_height * frame_depth;
		}

		bool operator ==(const FrameSize &other) const
		{
			return
				frame_width == other.frame_width &&
				frame_height == other.frame_height &&
				frame_depth == other.frame_depth;
		}

		bool operator !=(const FrameSize &other) const
		{
			return !(
				frame_width == other.frame_width &&
				frame_height == other.frame_height &&
				frame_depth == other.frame_depth
				);
		}
	};
}
