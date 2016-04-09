#pragma once

#include <atomic>
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
		std::atomic<bool> _isRunning;

		void query();
		void run();

	public:
		OvrFrameProducer();
		~OvrFrameProducer();

		void poll(unsigned char *dataLeft, unsigned char *dataRight);
		const OVR::OvrvisionPro* getCamera();
		const std::size_t getImageMemorySize();
	};
}
