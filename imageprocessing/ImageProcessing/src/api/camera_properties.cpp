#include <Unity/IUnityInterface.h>

#include "cameras/ActiveCamera.h"
#include "cameras/CameraSourceInterface.h"

#include "cameras/OvrvisionCameraSource.h" // for a few special camera properties

extern "C" UNITY_INTERFACE_EXPORT int GetCamWidth()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		return cam->GetFrameWidth();
	}
	else
	{
		return 0;
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamHeight()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();

	if (cam)
	{
		return cam->GetFrameHeight();
	}
	else
	{
		return 0;
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamChannels()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam.get() != nullptr)
	{
		return cam->GetFrameChannels();
	}
	else
	{
		return 0;
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamGain()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();

	if (cam)
	{
		return cam->GetCamGain();
	}
	else
	{
		return -1;
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamGain(const int val)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		cam->SetCamGain(val);
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamExposure()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		return cam->GetCamExposure();
	}
	else
	{
		return -1;
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamExposure(const int val)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		cam->SetCamExposure(val);
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamExposurePerSec(const float val)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		cam->SetCamExposurePerSec(val);
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamBLC()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		return cam->GetCamBLC();
	}
	else
	{
		return -1;
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamBLC(const int val)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		cam->SetCamBLC(val);
	}
}

extern "C" UNITY_INTERFACE_EXPORT bool GetCamAutoWhiteBalance()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		return cam->GetCamAutoWhiteBalance();
	}
	else
	{
		return false;
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamAutoWhiteBalance(const bool val)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		cam->SetCamAutoWhiteBalance(val);
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceR()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		return cam->GetCamWhiteBalanceR();
	}
	else
	{
		return -1;
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceR(const int val)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		cam->SetCamWhiteBalanceR(val);
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceG()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		return cam->GetCamWhiteBalanceG();
	}
	else
	{
		return -1;
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceG(const int val)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		cam->SetCamWhiteBalanceG(val);
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamWhiteBalanceB()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		return cam->GetCamWhiteBalanceB();
	}
	else
	{
		return -1;
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamWhiteBalanceB(const int val)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		cam->SetCamWhiteBalanceB(val);
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamFps()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		return cam->GetCamFps();
	}
	else
	{
		return -1;
	}
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
