#pragma once

#include <atomic>
#include <condition_variable>
#include <thread>
#include <mutex>
#include <ovrvision_pro.h>

namespace ImageProcessing
{
	class OvrFrameProducer
	{
	private:
		std::unique_ptr<OVR::OvrvisionPro> _ovrCamera;
		std::unique_ptr<unsigned char[]> _dataLeft, _dataRight;
		std::size_t _imgMemSize;

		std::thread _thread;
		std::mutex _mutex;
		std::condition_variable _frameNotifier;
		std::atomic<bool> _isRunning;
		std::atomic<int> _frameCounter = -1;

		void query();
		void run();

	public:
		OvrFrameProducer();
		~OvrFrameProducer();

		void poll(long &frameId, unsigned char *dataLeft, unsigned char *dataRight);
		OVR::OvrvisionPro* getCamera() const;
		std::size_t getImageMemorySize() const;
		long getCurrentFrameCount() const;
		std::condition_variable* getFrameNotifier();
	};
}
