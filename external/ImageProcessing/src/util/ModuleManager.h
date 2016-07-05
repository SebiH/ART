#pragma once

#include <map>
#include <memory>
#include <string>

#include "ThreadedModule.h"
#include "../framesource/IFrameSource.h"

namespace ImageProcessing
{
	class ModuleManager
	{
	private:
		std::shared_ptr<IFrameSource> _frameProducer;
		std::map<std::string, std::shared_ptr<ThreadedModule>> _createdModules;

	public:
		ModuleManager(const std::shared_ptr<IFrameSource> frameProducer);
		~ModuleManager();

		bool hasModule(const std::string &moduleName);
		std::shared_ptr<ThreadedModule> getOrCreateModule(const std::string &moduleName);

		std::shared_ptr<IFrameSource> getFrameSource() const;
		void triggerTextureUpdate();
	};
}
