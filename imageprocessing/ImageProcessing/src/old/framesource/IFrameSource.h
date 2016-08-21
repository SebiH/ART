#pragma once

#include <cstddef>
#include "../util/ImageInfo.h"

namespace ImageProcessing
{
	class IFrameSource
	{
	public:
		virtual ~IFrameSource() {}

		virtual void close() = 0;
		virtual ImageInfo poll(long &frameId, unsigned char *bufferLeft, unsigned char *bufferRight) = 0;
		virtual std::size_t getImageBufferSize() const = 0;

		// camera properties
		virtual int getFrameWidth() const = 0;
		virtual int getFrameHeight() const = 0;
		virtual int getFrameChannels() const = 0;
		virtual int getCamExposure() const = 0;
		virtual void setCamExposure(const int val) const = 0;
		virtual int getCamGain() const = 0;
		virtual void setCamGain(const int val) const = 0;
		virtual int getCamBLC() const = 0;
		virtual void setCamBLC(const int val) const = 0;
		virtual bool getCamAutoWhiteBalance() const = 0;
		virtual void setCamAutoWhiteBalance(const bool val) const = 0;
		virtual int getCamWhiteBalanceR() const = 0;
		virtual void setCamWhiteBalanceR(const int val) const = 0;
		virtual int getCamWhiteBalanceG() const = 0;
		virtual void setCamWhiteBalanceG(const int val) const = 0;
		virtual int getCamWhiteBalanceB() const = 0;
		virtual void setCamWhiteBalanceB(const int val) const = 0;
		virtual int getCamFps() const = 0;
		virtual bool isOpen() const = 0;
	};
}
