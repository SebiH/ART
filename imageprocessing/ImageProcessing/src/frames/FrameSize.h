#pragma once

#include <cstddef>

namespace ImageProcessing
{
	class FrameSize
	{
	public:
		int width;
		int height;
		int depth;

	public:
		FrameSize()
			: FrameSize(0, 0, 0)
		{

		}

		FrameSize(const int width, const int height, const int depth)
			: width(width),
			  height(height),
			  depth(depth)
		{ }

		virtual ~FrameSize() { };

		std::size_t inline BufferSize() const
		{
			return width * height * depth;
		}

		bool operator ==(const FrameSize &other) const
		{
			return
				width == other.width &&
				height == other.height &&
				depth == other.depth;
		}

		bool operator !=(const FrameSize &other) const
		{
			return !(
				width == other.width &&
				height == other.height &&
				depth == other.depth
				);
		}
	};
}
