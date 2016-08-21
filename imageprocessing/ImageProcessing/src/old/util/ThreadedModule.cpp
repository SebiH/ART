#include "ThreadedModule.h"

#include <utility>

#include "../util/PerformanceDebugging.h"

using namespace ImageProcessing;

ThreadedModule::ThreadedModule(const std::shared_ptr<IFrameSource> producer, std::unique_ptr<IProcessingModule> module)
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
	_isRunning = true;
	_thread = std::thread(&ThreadedModule::run, this);
}



bool ThreadedModule::isRunning() const
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
	std::unique_ptr<unsigned char[]> rawDataLeft(new unsigned char[_producer->getImageBufferSize()]);
	std::unique_ptr<unsigned char[]> rawDataRight(new unsigned char[_producer->getImageBufferSize()]);
	long currentFrameId = -1;

	while (_isRunning)
	{
		// TODO: wait if new frame is actually available
		// if (new frame is available) (else sleep)

		PERF_MEASURE(start)
		
		// load frames into local memory (for isolated processing)
		ImageInfo info = _producer->poll(currentFrameId, rawDataLeft.get(), rawDataRight.get());
		PERF_MEASURE(after_poll)

		auto result = _module->processImage(rawDataLeft.get(), rawDataRight.get(), info);
		PERF_MEASURE(processing)

		{
			PERF_MEASURE(before_lock)
			// write result into memory, so that it's instantly available if textureupdate is requested
			std::lock_guard<std::mutex> guard(_mutex);
			PERF_MEASURE(after_lock)
			_currentResults = std::move(result);
			_firstProcessingFinished = true;
			PERF_OUTPUT("Time spent waiting on lock for copying results: ", before_lock, after_lock)
		}

		PERF_MEASURE(end)

		PERF_OUTPUT("Total processing duration: ", start, end)
		PERF_OUTPUT("Polling duration", start, after_poll)
		PERF_OUTPUT("Processing duration", processing, end)
	}
}


void ThreadedModule::addTextureWriter(std::shared_ptr<ITextureWriter> writer)
{
	_writers.push_back(writer);
}

void ThreadedModule::removeTextureWriter(std::shared_ptr<ITextureWriter> writer)
{
	for (int i = 0; i < _writers.size(); i++)
	{
		if (_writers[i].get() == writer.get())
		{
			_writers.erase(_writers.begin() + i);
			return;
		}
	}
}


void ThreadedModule::updateTextures()
{

	if (!_firstProcessingFinished)
	{
		return;
	}

	PERF_MEASURE(start)

	std::lock_guard<std::mutex> guard(_mutex);
	for (auto writer : _writers)
	{
		writer->writeTexture(_currentResults);
	}

	PERF_MEASURE(end)
	PERF_OUTPUT("Time to write textures: ", start, end)

	// TODO: check if new texture are available to avoid writing the same texture twice
	// forEach registered texture writer
		// update texture
}

IProcessingModule* ThreadedModule::getProcessingModule() const
{
	return _module.get();
}
