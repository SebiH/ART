#pragma once

#include <cstddef>
#include <opencv2/core.hpp>

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

		int inline CvType() const
		{
			switch (depth)
			{
			case 1:
				return CV_8UC1;
			case 2:
				return CV_8UC2;
			case 3:
				return CV_8UC3;
			case 4:
				return CV_8UC4;
			default:
				throw std::exception("Invalid depth!");
			}
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
