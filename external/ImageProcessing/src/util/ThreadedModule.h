#pragma once

#include <atomic>
#include <mutex>
#include <thread>

#include "IProcessingModule.h"
#include "ITextureWriter.h"
#include "OvrFrameProducer.h"

namespace ImageProcessing
{
	class ThreadedModule
	{
	private:
		std::thread _thread;
		std::mutex _mutex;
		std::atomic<bool> _isRunning;

		const OvrFrameProducer *_producer;
		const IProcessingModule *_module;
		const ITextureWriter *_writer;

		void run();

	public:
		explicit ThreadedModule(const OvrFrameProducer &producer, const IProcessingModule &module, const ITextureWriter &writer);
		~ThreadedModule();

		void start();
		bool isRunning();
		void stop();

		void updateTextures();
	};

}
