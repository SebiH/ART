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
#include "..\framesource\NullFrameSource.h"
#include "..\processingmodule\IProcessingModule.h"
#include "..\processingmodule\RoiModule.h"
#include "..\texturewriter\ITextureWriter.h"
#include "..\texturewriter\OpenCvTextureWriter.h"
#include "..\texturewriter\UnityDX11TextureWriter.h"
#include "..\util\ThreadedModule.h"
#include "..\util\UnityUtils.h"

using namespace ImageProcessing;

// TODO refactor!
int _currentFrameSourceIdHack = -1;

std::shared_ptr<IFrameSource> _frameSource;
extern "C" UNITY_INTERFACE_EXPORT void SetFrameSource(int source);
bool _isInitialized;
std::unique_ptr<ModuleManager> g_moduleManager;

int idCounter = 0;
std::map<int, std::pair<std::shared_ptr<ThreadedModule>, std::shared_ptr<ITextureWriter>>> registeredTextureWriters;


void InitializeImageProcessing()
{
	if (_frameSource.get() == nullptr)
	{
		SetFrameSource(_currentFrameSourceIdHack);
	}

	if (!_isInitialized)
	{
		_isInitialized = true;
		g_moduleManager = std::make_unique<ModuleManager>(_frameSource);
	}
}

void ShutdownImageProcessing()
{
	if (_isInitialized)
	{
		_isInitialized = false;
		_frameSource->close();
		_frameSource.reset();
		g_moduleManager.reset();
	}
}


// Unnecessary within Unity!
extern "C" UNITY_INTERFACE_EXPORT void StartImageProcessing()
{
	// quick hack...
	SetFrameSource(_currentFrameSourceIdHack);

	InitializeImageProcessing();
}

// Do not use within Unity!
extern "C" UNITY_INTERFACE_EXPORT void UpdateTextures()
{
	g_moduleManager->triggerTextureUpdate();
}



extern "C" UNITY_INTERFACE_EXPORT void SetFrameSource(int sourceId)
{
	// TODO: this was hacked together quickly, could definitely use something better!
	if (_currentFrameSourceIdHack != sourceId || _frameSource.get() == nullptr)
	{
		_currentFrameSourceIdHack = sourceId;
		bool restartImageProcessing = false;

		if (_isInitialized)
		{
			restartImageProcessing = true;
			ShutdownImageProcessing();
		}
	
		switch (sourceId)
		{
		case 0:
			DebugLog("Using OpenCVFrameSource");
			_frameSource = std::make_shared<OpenCVFrameProducer>();
			break;

		case 1:
			DebugLog("Using LeapFrameSource");
			_frameSource = std::make_shared<LeapFrameSource>();
			break;



		case 2:
			DebugLog("Using OVRFrameSource @ 2560x1920 @15fps");
			_frameSource = std::make_shared<OvrFrameProducer>(OVR::Camprop::OV_CAM5MP_FULL);
			break;

		case 3:
			DebugLog("Using OVRFrameSource @ 1920x1080 @ 30fps");
			_frameSource = std::make_shared<OvrFrameProducer>(OVR::Camprop::OV_CAM5MP_FHD);
			break;

		case 4:
			DebugLog("Using OVRFrameSource @ 1280x960 @ 45fps");
			_frameSource = std::make_shared<OvrFrameProducer>(OVR::Camprop::OV_CAMHD_FULL);
			break;

		case 5:
			DebugLog("Using OVRFrameSource @ 960x950 @ 60fps");
			_frameSource = std::make_shared<OvrFrameProducer>(OVR::Camprop::OV_CAMVR_FULL);
			break;

		case 6:
			DebugLog("Using OVRFrameSource @ 1280x800 @ 60fps");
			_frameSource = std::make_shared<OvrFrameProducer>(OVR::Camprop::OV_CAMVR_WIDE);
			break;

		case 7:
			DebugLog("Using OVRFrameSource @ 640x480 @ 90fps");
			_frameSource = std::make_shared<OvrFrameProducer>(OVR::Camprop::OV_CAMVR_VGA);
			break;

		case 8:
			DebugLog("Using OVRFrameSource @ 320x240 @ 120fps");
			_frameSource = std::make_shared<OvrFrameProducer>(OVR::Camprop::OV_CAMVR_QVGA);
			break;



		default:
			DebugLog("Using NullFrameSource");
			_frameSource = std::make_shared<NullFrameSource>(640, 480);
			break;
		}

		if (restartImageProcessing)
		{
			InitializeImageProcessing();
		}
	}
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
