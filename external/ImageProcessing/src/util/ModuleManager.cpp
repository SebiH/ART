#include "ModuleManager.h"

#include "../processingmodule/RawImageModule.h"

using namespace ImageProcessing;

ModuleManager::ModuleManager(const std::shared_ptr<OvrFrameProducer> frameProducer)
	: _frameProducer(frameProducer)
{
}


ModuleManager::~ModuleManager()
{
}

std::shared_ptr<ThreadedModule> ModuleManager::getOrCreateModule(const std::string &moduleName)
{
	std::string modName(moduleName);
	std::shared_ptr<ThreadedModule> module;
	auto cam = _frameProducer->getCamera();

	bool isModuleRunning = _createdModules.count(modName) > 0;

	if (isModuleRunning)
	{
		module = _createdModules[modName];
	}
	else // module not running yet, start a new one
	{
		if (modName == "RawImage")
		{
			module = std::make_shared<ThreadedModule>(_frameProducer, std::unique_ptr<IProcessingModule>(new RawImageModule(cam->GetCamWidth(), cam->GetCamHeight(), cam->GetCamPixelsize())));
		}
		else // unknown module
		{
			// TODO: exception / error code?
		}

		_createdModules.insert({ modName, module });
		module->start();
	}

	return module;
}

void ModuleManager::triggerTextureUpdate()
{
	for (auto module : _createdModules)
	{
		module.second->updateTextures();
	}
}
