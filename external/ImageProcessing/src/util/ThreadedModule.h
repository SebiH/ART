#pragma once

#include <atomic>
#include <mutex>
#include <thread>
#include <vector>

#include "../processingmodule/IProcessingModule.h"
#include "../texturewriter/ITextureWriter.h"
#include "OvrFrameProducer.h"

namespace ImageProcessing
{
	class ThreadedModule
	{
	private:
		std::thread _thread;
		std::mutex _mutex;
		std::atomic<bool> _isRunning;

		const std::shared_ptr<OvrFrameProducer> _producer;
		const std::unique_ptr<IProcessingModule> _module;
		std::vector<std::shared_ptr<ITextureWriter>> _writers;

		bool _firstProcessingFinished = false;
		std::vector<std::unique_ptr<unsigned char[]>> _currentResults;


		void run();

	public:
		explicit ThreadedModule(const std::shared_ptr<OvrFrameProducer> producer, std::unique_ptr<IProcessingModule> module);
		~ThreadedModule();

		void start();
		bool isRunning();
		void stop();

		void addTextureWriter(std::shared_ptr<ITextureWriter> writer);
		void updateTextures();
	};

}
