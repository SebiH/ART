#pragma once

#include <cstddef>

namespace ImageProcessing
{
	class FrameMetaData
	{
	public:
		int FrameWidth;
		int FrameHeight;
		int FrameDepth;

	public:
		virtual ~FrameMetaData() { }

		std::size_t GetBufferSize() const
		{
			return FrameWidth * FrameHeight * FrameDepth;
		}

		bool operator ==(const FrameMetaData &other) const
		{
			return
				FrameWidth == other.FrameWidth &&
				FrameHeight == other.FrameHeight &&
				FrameDepth == other.FrameDepth;
		}

		bool operator !=(const FrameMetaData &other) const
		{
			return !(
				FrameWidth == other.FrameWidth &&
				FrameHeight == other.FrameHeight &&
				FrameDepth == other.FrameDepth
			);
		}

	};
}
