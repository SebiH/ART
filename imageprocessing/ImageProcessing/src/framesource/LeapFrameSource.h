#pragma once

#include <atomic>
#include <condition_variable>
#include <thread>
#include <mutex>
#include <leapmotion/Leap.h>

#include "IFrameSource.h"

namespace ImageProcessing
{
	class LeapFrameSource : public IFrameSource
	{
	private:
		std::unique_ptr<Leap::Controller> _camera;
		std::unique_ptr<unsigned char[]> _dataLeft, _dataRight;
		std::size_t _imgBufferSize;
		ImageInfo _imgInfo;

		std::thread _thread;
		std::mutex _mutex;
		std::condition_variable _frameNotifier;
		std::atomic<bool> _isRunning;
		std::atomic<int> _frameCounter = -1;

		void query();
		void run();

	public:
		LeapFrameSource();
		~LeapFrameSource();

		virtual void close();
		virtual ImageInfo poll(long &frameId, unsigned char *bufferLeft, unsigned char *bufferRight) override;
		virtual std::size_t getImageBufferSize() const override;

		// camera properties
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
