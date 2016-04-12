#pragma once

#include <map>
#include <memory>
#include <string>
#include <opencv2/highgui.hpp>

#include "processingmodule/IProcessingModule.h"
#include "processingmodule/RawImageModule.h"
#include "texturewriter/ITextureWriter.h"
#include "texturewriter/OpenCvTextureWriter.h"
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
	return 0.f;
}


extern "C" DllExport void SetCameraProperty(char *propName, float propVal)
{

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

extern "C" DllExport int RegisterOpenCVTextureWriter(char *modulename, char *windowname)
{
	std::string modName(modulename);
	std::shared_ptr<ThreadedModule> module;
	auto cam = frameProducer->getCamera();

	if (runningModules.count(modName))
	{
		module = runningModules[modName];
	}
	else // module not running yet, start a new one
	{
		if (modName == "RawImage")
		{
			auto cam = frameProducer->getCamera();
			module = std::make_shared<ThreadedModule>(frameProducer, std::unique_ptr<IProcessingModule>(new RawImageModule(cam->GetCamWidth(), cam->GetCamHeight(), cam->GetCamPixelsize())));
			runningModules.insert({ modName, module });
			module->start();
		}
		else
		{
			// TODO: exception / error code?
			return -1;
		}
	}

	module->addTextureWriter(std::make_shared<OpenCvTextureWriter>(std::string(windowname), cam->GetCamWidth(), cam->GetCamHeight(), 2));

	// TODO: return handle to deregister
	return -1;
}


extern "C" DllExport int RegisterDx11TexturePtr(char *moduleName, unsigned char *texturePtr)
{
	// if module doesn't exist, start it
	// append textureptr to module

	return -1;
}


extern "C" DllExport void DeregisterTexturePtr(int handle)
{

}
