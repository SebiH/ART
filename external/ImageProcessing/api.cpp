#pragma once

#define DllExport   __declspec( dllexport )

#include <memory>
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

	// store properties for later
	camWidth = ovrCamera.GetCamWidth();
	camHeight = ovrCamera.GetCamHeight();
}


extern "C" DllExport void Stop()
{
	ovrCamera.Close();
}


extern "C" DllExport float GetProperty(const char *prop)
{
	if (prop == "width")
	{
		return (float)ovrCamera.GetCamWidth();
	}
	else if (prop == "height")
	{
		return (float)ovrCamera.GetCamHeight();
	}
	else if (prop == "exposure")
	{
		return (float)ovrCamera.GetCameraExposure();
	}
	else if (prop == "gain")
	{
		return (float)ovrCamera.GetCameraGain();
	}
	else
	{
		cv::Mat test(cv::Size(200, 500), CV_64F);
		imshow("testwin", test);
		// TODO: throw warning about unknown prop
		return .05f;
	}
}


extern "C" DllExport void SetProperty(const char *prop, float value)
{
	// TODO: more props
	if (prop == "exposure")
	{
		ovrCamera.SetCameraExposure((int)value);
	}
	else if (prop == "gain")
	{
		ovrCamera.SetCameraGain((int)value);
	}
	else
	{
		// TODO: throw warning about unkown prop
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
