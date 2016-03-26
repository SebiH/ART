#pragma once

#define DllExport   __declspec( dllexport )

#include <opencv2/opencv.hpp>
#include <ovrvision_pro.h>

using namespace cv;

OVR::OvrvisionPro ovrCamera;
int camWidth, camHeight;

extern "C" DllExport void Start(int cameraMode = -1)
{
	OVR::Camprop camProp = (cameraMode == -1) ? OVR::OV_CAMVR_FULL : (OVR::Camprop)cameraMode;
	ovrCamera = OVR::OvrvisionPro();

	// TODO: error on failure?
	auto openSuccess = ovrCamera.Open(0, camProp);


	// default settings
	ovrCamera.SetCameraExposure(12960);
	ovrCamera.SetCameraGain(47);
	ovrCamera.SetCameraSyncMode(false);

	// store propertys for later
	camWidth = ovrCamera.GetCamWidth();
	camHeight = ovrCamera.GetCamHeight();
}


extern "C" DllExport void Stop()
{
	ovrCamera.Close();
}


extern "C" DllExport float GetProperty(std::string *property)
{
	if (*property == "width")
	{
		return (float)ovrCamera.GetCamWidth();
	}
	else if (*property == "height")
	{
		return (float)ovrCamera.GetCamHeight();
	}
	else if (*property == "exposure")
	{
		return (float)ovrCamera.GetCameraExposure();
	}
	else if (*property == "gain")
	{
		return (float)ovrCamera.GetCameraGain();
	}
	else
	{
		// TODO: throw warning about unknown property
		return .0f;
	}
}


extern "C" DllExport void SetProperty(std::string *property, float value)
{
	// TODO: more propertys
	if (*property == "exposure")
	{
		ovrCamera.SetCameraExposure((int)value);
	}
	else if (*property == "gain")
	{
		ovrCamera.SetCameraGain((int)value);
	}
	else
	{
		// TODO: throw warning about unkown property
	}
}


extern "C" DllExport void WriteTexture(unsigned char *leftUnityPtr, unsigned char *rightUnityPtr)
{
	if (ovrCamera.isOpen())
	{
		ovrCamera.PreStoreCamData(OVR::OV_CAMQT_DMS);

		unsigned char *leftImg = ovrCamera.GetCamImageBGRA(OVR::OV_CAMEYE_LEFT);
		unsigned char *rightImg = ovrCamera.GetCamImageBGRA(OVR::OV_CAMEYE_RIGHT);

		auto imgSize = camHeight * camWidth * 4; // RGleftUnityPtrBA -> 4 channels
		// possible memory leaks, only for internal use!
		memcpy_s(leftUnityPtr, imgSize, leftImg, imgSize);
		memcpy_s(rightUnityPtr, imgSize, rightUnityPtr, imgSize);
	}
}
