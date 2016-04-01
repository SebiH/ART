#pragma once

#define DllExport   __declspec( dllexport )

#include <d3d11.h>
#include <opencv2/opencv.hpp>
#include <ovrvision_pro.h>

#include <mutex>
#include <thread>

using namespace cv;

OVR::OvrvisionPro *ovrCamera;
int camWidth, camHeight;
bool hasStarted = false;

// thread-safe memory for storing image data
std::mutex imgMutex;
size_t tsImageMemorySize;
unsigned char *tsImageLeft;
unsigned char *tsImageRight;


extern "C" DllExport void OvrStart(int cameraMode = -1)
{
	if (hasStarted)
	{
		return;
	}

	hasStarted = true;
	OVR::Camprop camProp = (cameraMode == -1) ? OVR::OV_CAMVR_FULL : (OVR::Camprop)cameraMode;
	ovrCamera = new OVR::OvrvisionPro();

	// TODO: error on failure?
	auto openSuccess = ovrCamera->Open(0, camProp);

	// default settings
	ovrCamera->SetCameraExposure(12960);
	ovrCamera->SetCameraGain(47);
	ovrCamera->SetCameraSyncMode(false);

	// store properties for later
	camWidth = ovrCamera->GetCamWidth();
	camHeight = ovrCamera->GetCamHeight();

	// create memory for distributing memory across modules
	tsImageMemorySize = camWidth * camHeight * 4;
	tsImageLeft = new unsigned char[tsImageMemorySize];
	tsImageRight = new unsigned char[tsImageMemorySize];
}


extern "C" DllExport void OvrStop()
{
	if (ovrCamera->isOpen())
	{
		ovrCamera->Close();
	}

	delete ovrCamera;
	delete[] tsImageLeft;
	delete[] tsImageRight;

	hasStarted = false;
}


extern "C" DllExport float GetProperty(const char *name)
{
	std::string prop(name);

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


extern "C" DllExport void SetProperty(const char *name, float value)
{
	std::string prop(name);

	// TODO: more props
	if (prop == "exposure")
	{
		ovrCamera->SetCameraExposure((int)value);
	}
	else if (prop == "gain")
	{
		ovrCamera->SetCameraGain((int)value);
	}
	else
	{
		// TODO: throw warning about unkown prop
	}
}

static void FillTexture(unsigned char *texturePtr, unsigned char *data)
{
	ID3D11Texture2D* d3dtex = (ID3D11Texture2D*)texturePtr;
	ID3D11Device *g_D3D11Device;
	d3dtex->GetDevice(&g_D3D11Device);

	ID3D11DeviceContext* ctx = NULL;
	g_D3D11Device->GetImmediateContext(&ctx);

	D3D11_TEXTURE2D_DESC desc;
	d3dtex->GetDesc(&desc);

	ctx->UpdateSubresource(d3dtex, 0, NULL, data, desc.Width * 4, 0);

	ctx->Release();
}

extern "C" DllExport void WriteTexture(unsigned char *leftUnityPtr, unsigned char *rightUnityPtr)
{
	if (ovrCamera->isOpen())
	{
		ovrCamera->PreStoreCamData(OVR::OV_CAMQT_DMS);

		unsigned char *leftImg = ovrCamera->GetCamImageBGRA(OVR::OV_CAMEYE_LEFT);
		unsigned char *rightImg = ovrCamera->GetCamImageBGRA(OVR::OV_CAMEYE_RIGHT);

		FillTexture(leftUnityPtr, leftImg);
		FillTexture(rightUnityPtr, rightImg);

		std::lock_guard<std::mutex> guard(imgMutex);
		memcpy_s(tsImageLeft, tsImageMemorySize, leftImg, tsImageMemorySize);
		memcpy_s(tsImageRight, tsImageMemorySize, rightImg, tsImageMemorySize);
	}
}


// TODO: put this into module
unsigned char *roiLeft;
unsigned char *roiRight;

extern "C" DllExport void WriteROITexture(int startX, int startY, int width, int height, unsigned char *leftUnityPtr, unsigned char *rightUnityPtr)
{
	// TODO: maybe better to copy memory than to keep lock..?
	std::lock_guard<std::mutex> guard(imgMutex);
	cv::Mat leftMat(height, width, CV_8UC4, tsImageLeft);
	cv::Mat rightMat(height, width, CV_8UC4, tsImageRight);

	cv::Mat roiLeftMat(leftMat, cv::Rect(startX, startY, width, height));
	cv::Mat roiRightMat(rightMat, cv::Rect(startX, startY, width, height));

	FillTexture(leftUnityPtr, roiLeftMat.data);
	FillTexture(rightUnityPtr, roiRightMat.data);
}
// /TODO
