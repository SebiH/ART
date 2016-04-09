#include "ThreadedModule.h"

using namespace ImageProcessing;

ThreadedModule::ThreadedModule(const OvrFrameProducer &producer, const IProcessingModule &module, const ITextureWriter &writer)
	: _producer(&producer),
      _module(&module),
	  _writer(&writer)
{

}


ThreadedModule::~ThreadedModule()
{
	stop();
}


void ThreadedModule::start()
{
	_thread = std::thread(&run);
}



bool ThreadedModule::isRunning()
{

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
	while (_isRunning)
	{
		// wait for _producer to produce new frame
		// if (new frame is available)
		// load frames into module memory (for isolated processing)
		// let module process frame
		// write result into memory, so that it's instantly available if textureupdate is requested
	}
}


void ThreadedModule::updateTextures()
{
	// TODO: check if new texture are available to avoid writing the same texture twice
}
