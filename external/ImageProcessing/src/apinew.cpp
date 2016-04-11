#pragma once

#include <memory>

#include "OvrFrameProducer.h"

#define DllExport   __declspec( dllexport )


bool _isInitialized;

extern "C" DllExport void StartImageProcessing()
{
	if (!_isInitialized)
	{
		_isInitialized = true;
		// create ovrframeproducer
	}
}

extern "C" DllExport float GetCameraProperty(char *propName)
{
	return 0.f;
}

extern "C" DllExport void SetCameraProperty(char *propName, float propVal)
{

}


extern "C" DllExport void UpdateTextures()
{
	// foreach running module
	// etc etc
}


extern "C" DllExport int RegisterTexturePtr(char *moduleName)
{
	// if module doesn't exist, start it
	// append textureptr to module

	return -1;
}


extern "C" DllExport void DeregisterTexturePtr(int handle)
{

}
