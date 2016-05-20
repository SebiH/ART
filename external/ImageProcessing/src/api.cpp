#pragma once

#include <map>
#include <memory>
#include <string>
#include <utility>
#include <opencv2/highgui.hpp>
#include <Unity\IUnityInterface.h>

#include "framesource\IFrameSource.h"
#include "framesource\OpenCVFrameProducer.h"
#include "framesource\OvrFrameProducer.h"
#include "framesource\LeapFrameSource.h"
#include "processingmodule/IProcessingModule.h"
#include "processingmodule/RoiModule.h"
#include "texturewriter/ITextureWriter.h"
#include "texturewriter/OpenCvTextureWriter.h"
#include "texturewriter/UnityDX11TextureWriter.h"
#include "util/ModuleManager.h"
#include "util/ThreadedModule.h"

using namespace ImageProcessing;

bool _isInitialized;
std::shared_ptr<IFrameSource> frameProducer;
std::unique_ptr<ModuleManager> moduleManager;

int idCounter = 0;
std::map<int, std::pair<std::shared_ptr<ThreadedModule>, std::shared_ptr<ITextureWriter>>> registeredTextureWriters;

extern "C" UNITY_INTERFACE_EXPORT void StartImageProcessing()
{
	if (!_isInitialized)
	{
		_isInitialized = true;
		//frameProducer = std::make_shared<OpenCVFrameProducer>();
		//frameProducer = std::make_shared<LeapFrameSource>();
		frameProducer = std::make_shared<OvrFrameProducer>();
		moduleManager = std::unique_ptr<ModuleManager>(new ModuleManager(frameProducer));
	}
}


// Do not use within Unity!
extern "C" UNITY_INTERFACE_EXPORT void UpdateTextures()
{
	moduleManager->triggerTextureUpdate();
}


extern "C" UNITY_INTERFACE_EXPORT int OpenCvWaitKey(int delay)
{
	return cv::waitKey(delay);
}


extern "C" UNITY_INTERFACE_EXPORT int RegisterOpenCVTextureWriter(const char *moduleName, const char *windowname)
{
	auto module = moduleManager->getOrCreateModule(std::string(moduleName));
	auto textureWriter = std::make_shared<OpenCvTextureWriter>(std::string(windowname));
	module->addTextureWriter(textureWriter);

	auto id = idCounter++;
	registeredTextureWriters[id] = std::make_pair(module, textureWriter);

	return id;
}


extern "C" UNITY_INTERFACE_EXPORT int RegisterDx11TexturePtr(const char *moduleName, void* texturePtr, /* ProcessingOutput::Type */ const int textureType)
{
	auto module = moduleManager->getOrCreateModule(std::string(moduleName));

	auto textureWriter = std::make_shared<UnityDX11TextureWriter>(texturePtr, static_cast<ProcessingOutput::Type>(textureType));
	module->addTextureWriter(textureWriter);

	auto id = idCounter++;
	registeredTextureWriters[id] = std::make_pair(module, textureWriter);

	return id;
}


extern "C" UNITY_INTERFACE_EXPORT void DeregisterTexturePtr(const int handle)
{
	bool hasId = registeredTextureWriters.count(handle) > 0;

	if (hasId)
	{
		auto entry = registeredTextureWriters[handle];
		entry.first->removeTextureWriter(entry.second);
		registeredTextureWriters.erase(handle);
	}
}

extern "C" UNITY_INTERFACE_EXPORT void ChangeRoi(const int moduleHandle, const int x, const int y, const int width, const int height)
{
	if (moduleManager->hasModule("ROI"))
	{
		IProcessingModule *module = moduleManager->getOrCreateModule("ROI")->getProcessingModule();
		auto roiModule = static_cast<RoiModule*>(module);
		roiModule->setRegion(cv::Rect(x, y, width, height));
	}
}


// Camera properties

extern "C" UNITY_INTERFACE_EXPORT int GetCamWidth()
{
	return frameProducer->getFrameWidth();
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamHeight()
{
	return frameProducer->getFrameHeight();
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamChannels()
{
	return frameProducer->getFrameChannels();
}

extern "C" UNITY_INTERFACE_EXPORT float GetCamGain()
{
	return frameProducer->getCamGain();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamGain(const float val)
{
	frameProducer->setCamGain(val);
}

extern "C" UNITY_INTERFACE_EXPORT float GetCamExposure()
{
	return frameProducer->getCamExposure();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamExposure(const float val)
{
	frameProducer->setCamExposure(val);
}
