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
	if (hasStarted)
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

/// <summary>
///	Fetches a new camera image from the OVRvision cameras
/// </summary>
extern "C" DllExport void FetchImage()
{
	// TODO: maybe invoke automatically within C++ every x seconds instead of waiting for unity update?
	if (ovrCamera->isOpen())
	{
		ovrCamera->PreStoreCamData(OVR::OV_CAMQT_DMS);

		unsigned char *leftImg = ovrCamera->GetCamImageBGRA(OVR::OV_CAMEYE_LEFT);
		unsigned char *rightImg = ovrCamera->GetCamImageBGRA(OVR::OV_CAMEYE_RIGHT);

		std::lock_guard<std::mutex> guard(imgMutex);
		memcpy_s(tsImageLeft, tsImageMemorySize, leftImg, tsImageMemorySize);
		memcpy_s(tsImageRight, tsImageMemorySize, rightImg, tsImageMemorySize);
	}
}

extern "C" DllExport void WriteTexture(unsigned char *leftUnityPtr, unsigned char *rightUnityPtr)
{
	// TODO: maybe unnecessary, since only FetchImages should write threadsafe images,
	//		 and both cannot (?) be called at the same time
	std::lock_guard<std::mutex> guard(imgMutex);
	if (leftUnityPtr != NULL)
	{
		FillTexture(leftUnityPtr, tsImageLeft);
	}

	if (rightUnityPtr != NULL)
	{
		FillTexture(rightUnityPtr, tsImageRight);
	}

}


// TODO: put this into module
extern "C" DllExport void WriteROITexture(int startX, int startY, int width, int height, unsigned char *leftUnityPtr, unsigned char *rightUnityPtr)
{
	auto *imgLeft = new unsigned char[width * height * 4];
	auto *imgRight = new unsigned char[width * height * 4];

	auto *currRowRoiLeft = imgLeft;
	auto *currRowRoiRight = imgRight;
	auto *currRowSrcLeft = tsImageLeft + startX * 4 + camWidth * 4 * startY;
	auto *currRowSrcRight = tsImageRight + startX * 4 + camWidth * 4 * startY;

	// lock
	{
		std::lock_guard<std::mutex> guard(imgMutex);

		for (int i = 0; i < height; i++)
		{
			memcpy_s(currRowRoiLeft, width * 4, currRowSrcLeft, width * 4);
			currRowRoiLeft += width * 4;
			currRowSrcLeft += camWidth * 4;

			memcpy_s(currRowRoiRight, width * 4, currRowSrcRight, width * 4);
			currRowRoiRight += width * 4;
			currRowSrcRight += camWidth * 4;
		}
	}

	if (leftUnityPtr != NULL)
	{
		FillTexture(leftUnityPtr, imgLeft);
	}

	if (rightUnityPtr != NULL)
	{
		FillTexture(rightUnityPtr, imgRight);
	}

	delete[] imgLeft;
	delete[] imgRight;
}
// /TODO
