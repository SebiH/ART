#pragma once

#include "IFrameSource.h"

#include <memory>

namespace ImageProcessing
{
	class NullFrameSource : public IFrameSource
	{
	private:
		int dummyWidth;
		int dummyHeight;
		std::size_t dummyMemorySize;
		std::unique_ptr<unsigned char[]> dummyMemory;

	public:
		NullFrameSource(const int width, const int height);
		~NullFrameSource();

		virtual void close();
		virtual ImageInfo poll(long &frameId, unsigned char *bufferLeft, unsigned char *bufferRight) override;
		virtual std::size_t getImageBufferSize() const override;

		virtual int getFrameWidth() const override;
		virtual int getFrameHeight() const override;
		virtual int getFrameChannels() const override;
		virtual int getCamExposure() const override;
		virtual void setCamExposure(int val) const override;
		virtual int getCamGain() const override;
		virtual void setCamGain(int val) const override;
		virtual int getCamBLC() const override;
		virtual void setCamBLC(const int val) const override;
		virtual bool getCamAutoWhiteBalance() const override;
		virtual void setCamAutoWhiteBalance(const bool val) const override;
		virtual int getCamWhiteBalanceR() const override;
		virtual void setCamWhiteBalanceR(const int val) const override;
		virtual int getCamWhiteBalanceG() const override;
		virtual void setCamWhiteBalanceG(const int val) const override;
		virtual int getCamWhiteBalanceB() const override;
		virtual void setCamWhiteBalanceB(const int val) const override;
		virtual int getCamFps() const override;
		virtual bool isOpen() const override;
	};
}
