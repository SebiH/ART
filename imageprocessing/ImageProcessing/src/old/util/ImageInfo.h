#pragma once

#include <cstddef>

namespace ImageProcessing
{
	struct ImageInfo
	{
		int width;
		int height;
		int channels;
		int type;
		std::size_t bufferSize;

		ImageInfo() {}

		ImageInfo(int width, int height, int channels, int type)
			: width(width),
  			  height(height),
			  channels(channels),
			  type(type),
			  bufferSize(width * height * channels)
		{}
	};
}
