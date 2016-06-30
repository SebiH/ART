#include "common.h"

#include <map>
#include <string>
#include <utility>
#include <opencv2\highgui.hpp>
#include <Unity\IUnityInterface.h>

#include "..\framesource\IFrameSource.h"
#include "..\framesource\OpenCVFrameProducer.h"
#include "..\framesource\OvrFrameProducer.h"
#include "..\framesource\LeapFrameSource.h"
#include "..\processingmodule\IProcessingModule.h"
#include "..\processingmodule\RoiModule.h"
#include "..\texturewriter\ITextureWriter.h"
#include "..\texturewriter\OpenCvTextureWriter.h"
#include "..\texturewriter\UnityDX11TextureWriter.h"
#include "..\util\ThreadedModule.h"

using namespace ImageProcessing;

bool _isInitialized;
std::shared_ptr<IFrameSource> frameProducer;
std::unique_ptr<ModuleManager> g_moduleManager;

int idCounter = 0;
std::map<int, std::pair<std::shared_ptr<ThreadedModule>, std::shared_ptr<ITextureWriter>>> registeredTextureWriters;

void InitializeImageProcessing()
{
	if (!_isInitialized)
	{
		_isInitialized = true;
		//frameProducer = std::make_shared<OpenCVFrameProducer>();
		//frameProducer = std::make_shared<LeapFrameSource>();
		frameProducer = std::make_shared<OvrFrameProducer>();
		g_moduleManager = std::make_unique<ModuleManager>(frameProducer);
	}
}

void ShutdownImageProcessing()
{
	if (_isInitialized)
	{
		_isInitialized = false;
		frameProducer->close();
	}
}


// Unnecessary within Unity!
extern "C" UNITY_INTERFACE_EXPORT void StartImageProcessing()
{
	InitializeImageProcessing();
}

// Do not use within Unity!
extern "C" UNITY_INTERFACE_EXPORT void UpdateTextures()
{
	g_moduleManager->triggerTextureUpdate();
}


extern "C" UNITY_INTERFACE_EXPORT int OpenCvWaitKey(int delay)
{
	return cv::waitKey(delay);
}


extern "C" UNITY_INTERFACE_EXPORT int RegisterOpenCVTextureWriter(const char *moduleName, const char *windowname)
{
	auto module = g_moduleManager->getOrCreateModule(std::string(moduleName));
	auto textureWriter = std::make_shared<OpenCvTextureWriter>(std::string(windowname));
	module->addTextureWriter(textureWriter);

	auto id = idCounter++;
	registeredTextureWriters[id] = std::make_pair(module, textureWriter);

	return id;
}


extern "C" UNITY_INTERFACE_EXPORT int RegisterDx11TexturePtr(const char *moduleName, void* texturePtr, /* ProcessingOutput::Type */ const int textureType)
{
	auto module = g_moduleManager->getOrCreateModule(std::string(moduleName));

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
	if (g_moduleManager->hasModule("ROI"))
	{
		IProcessingModule *module = g_moduleManager->getOrCreateModule("ROI")->getProcessingModule();
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

extern "C" UNITY_INTERFACE_EXPORT int GetCamGain()
{
	return frameProducer->getCamGain();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamGain(const int val)
{
	frameProducer->setCamGain(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamExposure()
{
	return frameProducer->getCamExposure();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamExposure(const int val)
{
	frameProducer->setCamExposure(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamBLC()
{
	return frameProducer->getCamBLC();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamBLC(const int val)
{
	frameProducer->setCamBLC(val);
}

extern "C" UNITY_INTERFACE_EXPORT bool GetCamAutoWhiteBalance()
{
	return frameProducer->getCamAutoWhiteBalance();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamAutoWhiteBalance(const bool val)
{
	frameProducer->setCamAutoWhiteBalance(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceR()
{
	return frameProducer->getCamWhiteBalanceR();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceR(const int val)
{
	frameProducer->setCamWhiteBalanceR(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceG()
{
	return frameProducer->getCamWhiteBalanceG();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceG(const int val)
{
	frameProducer->setCamWhiteBalanceG(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceB()
{
	return frameProducer->getCamWhiteBalanceB();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceB(const int val)
{
	frameProducer->setCamWhiteBalanceB(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamFps()
{
	return frameProducer->getCamFps();
}

extern "C" UNITY_INTERFACE_EXPORT float GetHMDRightGap(const int at)
{
	if (auto ovrFrameProducer = dynamic_cast<OvrFrameProducer*>(frameProducer.get()))
	{
		return ovrFrameProducer->getHMDRightGap(at);
	}
	else
	{
		return 0.f;
	}
}

extern "C" UNITY_INTERFACE_EXPORT float GetCamFocalPoint()
{
	if (auto ovrFrameProducer = dynamic_cast<OvrFrameProducer*>(frameProducer.get()))
	{
		return ovrFrameProducer->getCamFocalPoint();
	}
	else
	{
		return 0.f;
	}
}
