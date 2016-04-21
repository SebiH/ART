#pragma once

#include <atomic>
#include <condition_variable>
#include <thread>
#include <mutex>
#include <Leap.h>

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

		virtual ImageInfo poll(long &frameId, unsigned char *bufferLeft, unsigned char *bufferRight) override;
		virtual std::size_t getImageBufferSize() const override;

		// camera properties
		virtual int getFrameWidth() const override;
		virtual int getFrameHeight() const override;
		virtual int getFrameChannels() const override;
		virtual float getCamExposure() const override;
		virtual void setCamExposure(float val) const override;
		virtual float getCamGain() const override;
		virtual void setCamGain(float val) const override;
		virtual bool isOpen() const override;
	};
}