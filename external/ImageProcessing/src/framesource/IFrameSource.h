#pragma once

#include <cstddef>
#include "../util/ImageInfo.h"

namespace ImageProcessing
{
	class IFrameSource
	{
	public:
		virtual ~IFrameSource() {}

		virtual ImageInfo poll(long &frameId, unsigned char *bufferLeft, unsigned char *bufferRight) = 0;
		virtual std::size_t getImageBufferSize() const = 0;

		// camera properties
		virtual int getFrameWidth() const = 0;
		virtual int getFrameHeight() const = 0;
		virtual int getFrameChannels() const = 0;
		virtual float getCamExposure() const = 0;
		virtual void setCamExposure(float val) const = 0;
		virtual float getCamGain() const = 0;
		virtual void setCamGain(float val) const = 0;
		virtual bool isOpen() const = 0;

	};
}
