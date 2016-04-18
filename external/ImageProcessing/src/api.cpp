#pragma once

#include <map>
#include <memory>
#include <string>
#include <utility>
#include <opencv2/highgui.hpp>

#include "framesource\IFrameSource.h"
#include "framesource\OpenCVFrameProducer.h"
#include "framesource\OvrFrameProducer.h"
#include "processingmodule/IProcessingModule.h"
#include "processingmodule/RoiModule.h"
#include "texturewriter/ITextureWriter.h"
#include "texturewriter/OpenCvTextureWriter.h"
#include "texturewriter/UnityDX11TextureWriter.h"
#include "util/ModuleManager.h"
#include "util/ThreadedModule.h"

#define DllExport   __declspec( dllexport )

using namespace ImageProcessing;

bool _isInitialized;
std::shared_ptr<OpenCVFrameProducer> frameProducer;
std::unique_ptr<ModuleManager> moduleManager;

int idCounter = 0;
std::map<int, std::pair<std::shared_ptr<ThreadedModule>, std::shared_ptr<ITextureWriter>>> registeredTextureWriters;

extern "C" DllExport void StartImageProcessing()
{
	if (!_isInitialized)
	{
		_isInitialized = true;
		frameProducer = std::make_shared<OpenCVFrameProducer>();
		moduleManager = std::unique_ptr<ModuleManager>(new ModuleManager(frameProducer));
	}
}


extern "C" DllExport float GetCameraProperty(char *propName)
{
	std::string prop(propName);

	if (prop == "width")
	{
		return (float)frameProducer->getFrameWidth();
	}
	else if (prop == "height")
	{
		return (float)frameProducer->getFrameHeight();
	}
	else if (prop == "exposure")
	{
		return (float)frameProducer->getCamExposure();
	}
	else if (prop == "gain")
	{
		return (float)frameProducer->getCamGain();
	}
	else if (prop == "isOpen")
	{
		return (frameProducer->isOpen()) ? 10.0f : 0.0f; // TODO: better return value?
	}
	else
	{
		// TODO: throw warning about unknown prop
		return -1.f;
	}
}


extern "C" DllExport void SetCameraProperty(char *propName, float propVal)
{
	std::string prop(propName);

	// TODO: more props
	if (prop == "exposure")
	{
		frameProducer->setCamExposure(propVal);
	}
	else if (prop == "gain")
	{
		frameProducer->setCamGain(propVal);
	}
	else
	{
		// TODO: throw warning about unkown prop
	}
}


extern "C" DllExport void UpdateTextures()
{
	moduleManager->triggerTextureUpdate();
}


extern "C" DllExport int OpenCvWaitKey(int delay)
{
	return cv::waitKey(delay);
}


extern "C" DllExport int RegisterOpenCVTextureWriter(char *moduleName, char *windowname)
{
	auto module = moduleManager->getOrCreateModule(std::string(moduleName));
	auto textureWriter = std::make_shared<OpenCvTextureWriter>(std::string(windowname));
	module->addTextureWriter(textureWriter);

	auto id = idCounter++;
	registeredTextureWriters[id] = std::make_pair(module, textureWriter);

	return id;
}


extern "C" DllExport int RegisterDx11TexturePtr(char *moduleName, unsigned char *texturePtr, /* ProcessingOutput::Type */ int textureType)
{
	auto module = moduleManager->getOrCreateModule(std::string(moduleName));

	auto textureWriter = std::make_shared<UnityDX11TextureWriter>(texturePtr, static_cast<ProcessingOutput::Type>(textureType));
	module->addTextureWriter(textureWriter);

	auto id = idCounter++;
	registeredTextureWriters[id] = std::make_pair(module, textureWriter);

	return id;
}


extern "C" DllExport void DeregisterTexturePtr(int handle)
{
	bool hasId = registeredTextureWriters.count(handle) > 0;

	if (hasId)
	{
		auto entry = registeredTextureWriters[handle];
		entry.first->removeTextureWriter(entry.second);
		registeredTextureWriters.erase(handle);
	}
}

extern "C" DllExport void ChangeRoi(int moduleHandle, int x, int y, int width, int height)
{
	if (moduleManager->hasModule("ROI"))
	{
		IProcessingModule *module = moduleManager->getOrCreateModule("ROI")->getProcessingModule();
		auto roiModule = static_cast<RoiModule*>(module);
		roiModule->setRegion(cv::Rect(x, y, width, height));
	}
}
