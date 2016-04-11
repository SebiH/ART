#include "ThreadedModule.h"

#include <utility>

using namespace ImageProcessing;

ThreadedModule::ThreadedModule(const std::shared_ptr<OvrFrameProducer> producer, std::unique_ptr<IProcessingModule> module)
	: _producer(producer),
      _module(std::move(module))
{

}


ThreadedModule::~ThreadedModule()
{
	stop();
}


void ThreadedModule::start()
{
	_thread = std::thread(&ThreadedModule::run, this);
}



bool ThreadedModule::isRunning()
{
	return _isRunning;
}


void ThreadedModule::stop()
{
	if (_isRunning)
	{
		_isRunning = false;
		_thread.join();
	}
}


void ThreadedModule::run()
{
	std::unique_ptr<unsigned char[]> rawDataLeft(new unsigned char[_producer->getImageMemorySize()]);
	std::unique_ptr<unsigned char[]> rawDataRight(new unsigned char[_producer->getImageMemorySize()]);

	while (_isRunning)
	{
		// TODO: wait if new frame is actually available
		// if (new frame is available) (else sleep)

		// load frames into local memory (for isolated processing)
		_producer->poll(rawDataLeft.get(), rawDataRight.get());
		auto result = _module->processImage(rawDataLeft.get(), rawDataRight.get());

		{
			// write result into memory, so that it's instantly available if textureupdate is requested
			std::lock_guard<std::mutex> guard(_mutex);
			_currentResults = std::move(result);
		}
	}
}


void ThreadedModule::addTextureWriter(std::shared_ptr<ITextureWriter> writer)
{
	_writers.push_back(std::move(writer));
}


void ThreadedModule::updateTextures()
{
	std::lock_guard<std::mutex> guard(_mutex);
	for (auto writer : _writers)
	{
	}
	// TODO: check if new texture are available to avoid writing the same texture twice
	// forEach registered texture writer
		// update texture
}
