#pragma once

#include <map>
#include <memory>
#include <string>
#include <opencv2/highgui.hpp>

#include "processingmodule/IProcessingModule.h"
#include "processingmodule/RawImageModule.h"
#include "texturewriter/ITextureWriter.h"
#include "texturewriter/OpenCvTextureWriter.h"
#include "texturewriter/UnityDX11TextureWriter.h"
#include "util/ThreadedModule.h"
#include "util/OvrFrameProducer.h"

#define DllExport   __declspec( dllexport )

using namespace ImageProcessing;

bool _isInitialized;
std::shared_ptr<OvrFrameProducer> frameProducer;

extern "C" DllExport void StartImageProcessing()
{
	if (!_isInitialized)
	{
		_isInitialized = true;
		frameProducer = std::shared_ptr<OvrFrameProducer>(new OvrFrameProducer());
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


std::map<std::string, std::shared_ptr<ThreadedModule>> runningModules;

extern "C" DllExport void UpdateTextures()
{
	for (auto module : runningModules)
	{
		module.second->updateTextures();
	}
}


extern "C" DllExport int OpenCvWaitKey(int delay)
{
	return cv::waitKey(delay);
}


std::shared_ptr<ThreadedModule> getModule(char *moduleName)
{
	std::string modName(moduleName);
	std::shared_ptr<ThreadedModule> module;
	auto cam = frameProducer->getCamera();

	bool isModuleRunning = runningModules.count(modName) > 0;

	if (isModuleRunning)
	{
		module = runningModules[modName];
	}
	else // module not running yet, start a new one
	{
		if (modName == "RawImage")
		{
			auto cam = frameProducer->getCamera();
			module = std::make_shared<ThreadedModule>(frameProducer, std::unique_ptr<IProcessingModule>(new RawImageModule(cam->GetCamWidth(), cam->GetCamHeight(), cam->GetCamPixelsize())));
		}
		else // unknown module
		{
			// TODO: exception / error code?
		}

		runningModules.insert({ modName, module });
		module->start();
	}

	return module;
}


extern "C" DllExport int RegisterOpenCVTextureWriter(char *moduleName, char *windowname)
{
	auto cam = frameProducer->getCamera();
	auto module = getModule(moduleName);
	module->addTextureWriter(std::make_shared<OpenCvTextureWriter>(std::string(windowname), cam->GetCamWidth(), cam->GetCamHeight(), 2));

	// TODO: return handle to deregister
	return -1;
}


extern "C" DllExport int RegisterDx11TexturePtr(char *moduleName, int texturePtrCount, unsigned char **texturePtrs)
{
	auto module = getModule(moduleName);

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
