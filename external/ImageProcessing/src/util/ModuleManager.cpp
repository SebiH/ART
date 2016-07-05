#include "ModuleManager.h"

#include <utility>

#include "../processingmodule/RawImageModule.h"
#include "../processingmodule/RoiModule.h"

using namespace ImageProcessing;

ModuleManager::ModuleManager(const std::shared_ptr<IFrameSource> frameProducer)
	: _frameProducer(frameProducer)
{
}


ModuleManager::~ModuleManager()
{
}

std::shared_ptr<ThreadedModule> ModuleManager::getOrCreateModule(const std::string &moduleName)
{
	std::shared_ptr<ThreadedModule> module;

	if (hasModule(moduleName))
	{
		module = _createdModules[moduleName];
	}
	else // module not running yet, start a new one
	{
		std::unique_ptr<IProcessingModule> processingModule;

		if (moduleName == "RawImage")
		{
			processingModule = std::make_unique<RawImageModule>();
		}
		else if (moduleName == "ROI")
		{
			processingModule = std::make_unique<RoiModule>(cv::Rect(0, 0, _frameProducer->getFrameWidth(), _frameProducer->getFrameHeight()));
		}
		else // unknown module
		{
			// TODO: exception / error code?
		}

		module = std::make_shared<ThreadedModule>(_frameProducer, std::move(processingModule));
		_createdModules.insert({ moduleName, module });
		module->start();
	}

	return module;
}

std::shared_ptr<IFrameSource> ImageProcessing::ModuleManager::getFrameSource() const
{
	return _frameProducer;
}

void ModuleManager::triggerTextureUpdate()
{
	for (auto pair : _createdModules)
	{
		pair.second->updateTextures();
	}
}

bool ModuleManager::hasModule(const std::string &moduleName)
{
	return _createdModules.count(moduleName) > 0;
}
