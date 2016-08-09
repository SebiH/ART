#pragma once

#include <memory>
#include <opencv2/core.hpp>

#include "IFrameSource.h"

namespace ImageProcessing
{
	class DummyFrameSource : public IFrameSource
	{
	private:
		cv::Mat _img;
		ImageInfo _imgInfo;
		std::unique_ptr<unsigned char[]> _imgBuffer;

	public:
		DummyFrameSource();
		~DummyFrameSource();

		virtual void close() override;
		virtual ImageInfo poll(long & frameId, unsigned char * bufferLeft, unsigned char * bufferRight) override;
		virtual std::size_t getImageBufferSize() const override;

		/*
		 *	Camera Properties
	     */
		virtual bool isOpen() const override;

		virtual int getFrameWidth() const override;
		virtual int getFrameHeight() const override;
		virtual int getFrameChannels() const override;
		virtual int getCamExposure() const override;
		virtual void setCamExposure(const int val) const override;
		virtual int getCamGain() const override;
		virtual void setCamGain(const int val) const override;
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
	};
}
