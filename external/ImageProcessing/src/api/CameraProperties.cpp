#include <Unity\IUnityInterface.h>

#include "..\framesource\IFrameSource.h"
#include "..\framesource\OvrFrameProducer.h"

#include "common.h"


extern "C" UNITY_INTERFACE_EXPORT int GetCamWidth()
{
	return g_moduleManager->getFrameSource()->getFrameWidth();
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamHeight()
{
	return g_moduleManager->getFrameSource()->getFrameHeight();
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamChannels()
{
	return g_moduleManager->getFrameSource()->getFrameChannels();
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamGain()
{
	return g_moduleManager->getFrameSource()->getCamGain();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamGain(const int val)
{
	g_moduleManager->getFrameSource()->setCamGain(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamExposure()
{
	return g_moduleManager->getFrameSource()->getCamExposure();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamExposure(const int val)
{
	g_moduleManager->getFrameSource()->setCamExposure(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamBLC()
{
	return g_moduleManager->getFrameSource()->getCamBLC();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamBLC(const int val)
{
	g_moduleManager->getFrameSource()->setCamBLC(val);
}

extern "C" UNITY_INTERFACE_EXPORT bool GetCamAutoWhiteBalance()
{
	return g_moduleManager->getFrameSource()->getCamAutoWhiteBalance();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamAutoWhiteBalance(const bool val)
{
	g_moduleManager->getFrameSource()->setCamAutoWhiteBalance(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceR()
{
	return g_moduleManager->getFrameSource()->getCamWhiteBalanceR();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceR(const int val)
{
	g_moduleManager->getFrameSource()->setCamWhiteBalanceR(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceG()
{
	return g_moduleManager->getFrameSource()->getCamWhiteBalanceG();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceG(const int val)
{
	g_moduleManager->getFrameSource()->setCamWhiteBalanceG(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceB()
{
	return g_moduleManager->getFrameSource()->getCamWhiteBalanceB();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceB(const int val)
{
	g_moduleManager->getFrameSource()->setCamWhiteBalanceB(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamFps()
{
	return g_moduleManager->getFrameSource()->getCamFps();
}

extern "C" UNITY_INTERFACE_EXPORT float GetHMDRightGap(const int at)
{
	auto frameSource = g_moduleManager->getFrameSource();
	if (auto ovrFrameProducer = dynamic_cast<ImageProcessing::OvrFrameProducer*>(frameSource.get()))
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
	auto frameSource = g_moduleManager->getFrameSource();
	if (auto ovrFrameProducer = dynamic_cast<ImageProcessing::OvrFrameProducer*>(frameSource.get()))
	{
		return ovrFrameProducer->getCamFocalPoint();
	}
	else
	{
		return 0.f;
	}
}
