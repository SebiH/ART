#include <Unity/IUnityInterface.h>

#include "cameras/ActiveCamera.h"
#include "cameras/CameraSourceInterface.h"

#include "cameras/OvrvisionCameraSource.h" // for a few special camera properties

extern "C" UNITY_INTERFACE_EXPORT int GetCamWidth()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetFrameWidth();
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamHeight()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetFrameHeight();
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamChannels()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetFrameChannels();
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamGain()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetCamGain();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamGain(const int val)
{
	ImageProcessing::ActiveCamera::Instance()->GetSource()->SetCamGain(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamExposure()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetCamExposure();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamExposure(const int val)
{
	ImageProcessing::ActiveCamera::Instance()->GetSource()->SetCamExposure(val);
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamExposurePerSec(const float val)
{
	ImageProcessing::ActiveCamera::Instance()->GetSource()->SetCamExposurePerSec(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamBLC()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetCamBLC();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamBLC(const int val)
{
	ImageProcessing::ActiveCamera::Instance()->GetSource()->SetCamBLC(val);
}

extern "C" UNITY_INTERFACE_EXPORT bool GetCamAutoWhiteBalance()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetCamAutoWhiteBalance();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamAutoWhiteBalance(const bool val)
{
	ImageProcessing::ActiveCamera::Instance()->GetSource()->SetCamAutoWhiteBalance(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceR()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetCamWhiteBalanceR();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceR(const int val)
{
	ImageProcessing::ActiveCamera::Instance()->GetSource()->SetCamWhiteBalanceR(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceG()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetCamWhiteBalanceG();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceG(const int val)
{
	ImageProcessing::ActiveCamera::Instance()->GetSource()->SetCamWhiteBalanceG(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceB()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetCamWhiteBalanceB();
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceB(const int val)
{
	ImageProcessing::ActiveCamera::Instance()->GetSource()->SetCamWhiteBalanceB(val);
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamFps()
{
	return ImageProcessing::ActiveCamera::Instance()->GetSource()->GetCamFps();
}

extern "C" UNITY_INTERFACE_EXPORT float GetHMDRightGap(const int at)
{
	auto cam_source = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (auto ovr_source = dynamic_cast<ImageProcessing::OvrvisionCameraSource*>(cam_source.get()))
	{
		return ovr_source->GetHMDRightGap(at);
	}
	else
	{
		return 0.f;
	}
}

extern "C" UNITY_INTERFACE_EXPORT float GetCamFocalPoint()
{
	auto cam_source = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (auto ovr_source = dynamic_cast<ImageProcessing::OvrvisionCameraSource*>(cam_source.get()))
	{
		return ovr_source->GetCamFocalPoint();
	}
	else
	{
		return 0.f;
	}
}


extern "C" UNITY_INTERFACE_EXPORT int GetProcessingMode()
{
	auto cam_source = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (auto ovr_source = dynamic_cast<ImageProcessing::OvrvisionCameraSource*>(cam_source.get()))
	{
		return static_cast<int>(ovr_source->GetProcessingMode());
	}
	else
	{
		return 0;
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetProcessingMode(int mode)
{
	auto cam_source = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (auto ovr_source = dynamic_cast<ImageProcessing::OvrvisionCameraSource*>(cam_source.get()))
	{
		switch (mode)
		{
		case 0:
			ovr_source->SetProcessingMode(OVR::Camqt::OV_CAMQT_DMSRMP);
			break;

		case 1:
			ovr_source->SetProcessingMode(OVR::Camqt::OV_CAMQT_DMS);
			break;

		case 2:
		default:
			ovr_source->SetProcessingMode(OVR::Camqt::OV_CAMQT_NONE);
			break;
		}
	}
}
