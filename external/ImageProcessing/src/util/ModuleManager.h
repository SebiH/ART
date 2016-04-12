#pragma once

#include <map>
#include <memory>
#include <string>

#include "ThreadedModule.h"
#include "OvrFrameProducer.h"

namespace ImageProcessing
{
	class ModuleManager
	{
	private:
		std::shared_ptr<OvrFrameProducer> _frameProducer;
		std::map<std::string, std::shared_ptr<ThreadedModule>> _createdModules;

	public:
		ModuleManager(const std::shared_ptr<OvrFrameProducer> frameProducer);
		~ModuleManager();

		std::shared_ptr<ThreadedModule> getOrCreateModule(const std::string &moduleName);
		void triggerTextureUpdate();
	};
}
