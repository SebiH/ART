#pragma once

#include <map>
#include <memory>
#include <string>
#include <opencv2/highgui.hpp>

#include "processingmodule/IProcessingModule.h"
#include "processingmodule/RoiModule.h"
#include "texturewriter/ITextureWriter.h"
#include "texturewriter/OpenCvTextureWriter.h"
#include "texturewriter/UnityDX11TextureWriter.h"
#include "util/ModuleManager.h"
#include "util/OvrFrameProducer.h"
#include "util/ThreadedModule.h"

#define DllExport   __declspec( dllexport )

using namespace ImageProcessing;

bool _isInitialized;
std::shared_ptr<OvrFrameProducer> frameProducer;
std::unique_ptr<ModuleManager> moduleManager;

extern "C" DllExport void StartImageProcessing()
{
	if (!_isInitialized)
	{
		_isInitialized = true;
		frameProducer = std::make_shared<OvrFrameProducer>();
		moduleManager = std::unique_ptr<ModuleManager>(new ModuleManager(frameProducer));
	}
}


extern "C" DllExport float GetCameraProperty(char *propName)
{
	std::string prop(propName);
	auto ovrCamera = frameProducer->getCamera();

	if (prop == "width")
	{
		return (float)ovrCamera->GetCamWidth();
	}
	else if (prop == "height")
	{
		return (float)ovrCamera->GetCamHeight();
	}
	else if (prop == "exposure")
	{
		return (float)ovrCamera->GetCameraExposure();
	}
	else if (prop == "gain")
	{
		return (float)ovrCamera->GetCameraGain();
	}
	else if (prop == "isOpen")
	{
		return (ovrCamera->isOpen()) ? 10.0f : 0.0f; // TODO: better return value?
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
	auto ovrCamera = frameProducer->getCamera();

	// TODO: more props
	if (prop == "exposure")
	{
		ovrCamera->SetCameraExposure((int)propVal);
	}
	else if (prop == "gain")
	{
		ovrCamera->SetCameraGain((int)propVal);
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
	module->addTextureWriter(std::make_shared<OpenCvTextureWriter>(std::string(windowname)));

	// TODO: return handle to deregister
	return -1;
}


extern "C" DllExport int RegisterDx11TexturePtr(char *moduleName, int texturePtrCount, unsigned char **texturePtrs)
{
	auto module = moduleManager->getOrCreateModule(std::string(moduleName));

	std::vector<unsigned char *> texturePtrList;

	for (int i = 0; i < texturePtrCount; i++)
	{
		texturePtrList.push_back(texturePtrs[i]);
	}

	module->addTextureWriter(std::make_shared<UnityDX11TextureWriter>(texturePtrList));

	// TODO: return handle to deregister
	return -1;
}


extern "C" DllExport void DeregisterTexturePtr(int handle)
{

}

extern "C" DllExport void ChangeRoi(int moduleHandle, int x, int y, int width, int height)
{
	IProcessingModule *module = moduleManager->getOrCreateModule("ROI")->getProcessingModule();
	auto roiModule = dynamic_cast<RoiModule*>(module);
	roiModule->setRegion(cv::Rect(x, y, width, height));
}
