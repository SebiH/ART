#pragma once

#define DllExport   __declspec( dllexport )

#include <d3d11.h>
#include <opencv2/opencv.hpp>
#include <ovrvision_pro.h>

#include <memory>
#include <mutex>
#include <thread>

using namespace cv;

std::unique_ptr<OVR::OvrvisionPro> ovrCamera;
int camWidth, camHeight;
bool hasStarted = false;

bool keepExperimentalThreadRunning;
long frameCounter = 0;

// thread-safe memory for storing image data
std::mutex imgMutex;
std::mutex experimentalMutex;
size_t tsImageMemorySize;
std::unique_ptr<unsigned char[]> tsImageLeft;
std::unique_ptr<unsigned char[]> tsImageRight;
std::unique_ptr<unsigned char[]> tsExperimentalStereoImageData;

std::unique_ptr<std::thread> experimentalThread;
void TestThread();
bool initialized_once = false;

extern "C" DllExport void OvrStart(int cameraMode = -1)
{
	if (hasStarted)
	{
		return;
	}

	hasStarted = true;
	OVR::Camprop camProp = (cameraMode == -1) ? OVR::OV_CAMVR_FULL : (OVR::Camprop)cameraMode;

	if (!initialized_once)
	{
		ovrCamera = std::unique_ptr<OVR::OvrvisionPro>(new OVR::OvrvisionPro());
		initialized_once = true;
	}

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
	tsImageLeft = std::unique_ptr<unsigned char[]>(new unsigned char[tsImageMemorySize]);
	tsImageRight = std::unique_ptr<unsigned char[]>(new unsigned char[tsImageMemorySize]);

	// experimental
	tsExperimentalStereoImageData = std::unique_ptr<unsigned char[]>(new unsigned char[tsImageMemorySize]);
	keepExperimentalThreadRunning = true;
	//experimentalThread = std::unique_ptr<std::thread>(new std::thread(TestThread));
	frameCounter = 0;
}

extern "C" DllExport void OvrStop()
{
	if (hasStarted)
	{
		if (ovrCamera->isOpen())
		{
			ovrCamera->Close();
		}

		hasStarted = false;

		keepExperimentalThreadRunning = false;
		//experimentalThread->join();
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

static void FillTexture(unsigned char *texturePtr, const unsigned char *data)
{
	ID3D11Texture2D* d3dtex = (ID3D11Texture2D*)texturePtr;
	ID3D11Device *g_D3D11Device;
	d3dtex->GetDevice(&g_D3D11Device);

	ID3D11DeviceContext* ctx = NULL;
	g_D3D11Device->GetImmediateContext(&ctx);

	D3D11_TEXTURE2D_DESC desc;
	d3dtex->GetDesc(&desc);

	// TODO: store metadata to avoid unnecessary updates in case frame hasn't changed?
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
		memcpy_s(tsImageLeft.get(), tsImageMemorySize, leftImg, tsImageMemorySize);
		memcpy_s(tsImageRight.get(), tsImageMemorySize, rightImg, tsImageMemorySize);
		frameCounter++;
	}
}

extern "C" DllExport void WriteTexture(unsigned char *leftUnityPtr, unsigned char *rightUnityPtr)
{
	// TODO: maybe unnecessary, since only FetchImages should write threadsafe images,
	//		 and both cannot (?) be called at the same time
	std::lock_guard<std::mutex> guard(imgMutex);
	if (leftUnityPtr != NULL)
	{
		FillTexture(leftUnityPtr, tsImageLeft.get());
	}

	if (rightUnityPtr != NULL)
	{
		FillTexture(rightUnityPtr, tsImageRight.get());
	}

}


// TODO: put this into module
extern "C" DllExport void WriteROITexture(int startX, int startY, int width, int height, unsigned char *leftUnityPtr, unsigned char *rightUnityPtr)
{
	auto imgLeft = std::unique_ptr<unsigned char[]>(new unsigned char[width * height * 4]);
	auto imgRight = std::unique_ptr<unsigned char[]>(new unsigned char[width * height * 4]);

	auto *currRowRoiLeft = imgLeft.get();
	auto *currRowRoiRight = imgRight.get();
	auto *currRowSrcLeft = tsImageLeft.get() + startX * 4 + camWidth * 4 * startY;
	auto *currRowSrcRight = tsImageRight.get() + startX * 4 + camWidth * 4 * startY;

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
		FillTexture(leftUnityPtr, imgLeft.get());
	}

	if (rightUnityPtr != NULL)
	{
		FillTexture(rightUnityPtr, imgRight.get());
	}
}
// /TODO






std::vector<unsigned char *> experimentalTexturePtrs;

extern "C" DllExport void RegisterExperimentalTexturePtr(unsigned char *ptr)
{
	experimentalTexturePtrs.push_back(ptr);
}

extern "C" DllExport void UpdateExperimentalTexturePtr()
{
	std::lock_guard<std::mutex> guard(experimentalMutex);
	for (const auto &ptr : experimentalTexturePtrs)
	{
		FillTexture(ptr, tsExperimentalStereoImageData.get());
	}
}



void TestThread()
{
	long currentFrame = -1;
	std::unique_ptr<unsigned char[]> localImgDataLeft(new unsigned char[tsImageMemorySize]);
	std::unique_ptr<unsigned char[]> localImgDataRight(new unsigned char[tsImageMemorySize]);
	std::unique_ptr<unsigned char[]> playground(new unsigned char[tsImageMemorySize]);


	cv::Mat matLeft(cv::Size(camWidth, camHeight), CV_8UC4, localImgDataLeft.get());
	cv::Mat matRight(cv::Size(camWidth, camHeight), CV_8UC4, localImgDataRight.get());

	while (keepExperimentalThreadRunning)
	{
		if (currentFrame < frameCounter)
		{
			// new image is available! start processing
			{
				std::lock_guard<std::mutex> guard(imgMutex);
				memcpy(localImgDataLeft.get(), tsImageLeft.get(), tsImageMemorySize);
				memcpy(localImgDataRight.get(), tsImageRight.get(), tsImageMemorySize);
				currentFrame = frameCounter;
			}

			// process-heavy method for testing..
			for (int i = 0; i < tsImageMemorySize; )
			{
				playground[i] = localImgDataLeft[i];
				playground[i + 1] = localImgDataLeft[i + 1];
				playground[i + 2] = localImgDataLeft[i + 2];
				playground[i + 3] = localImgDataLeft[i + 3];
				i += 4;

				if (i + 4 < tsImageMemorySize)
				{
					playground[i] = localImgDataRight[i];
					playground[i + 1] = localImgDataRight[i + 1];
					playground[i + 2] = localImgDataRight[i + 2];
					playground[i + 3] = localImgDataRight[i + 3];
					i += 4;
				}
			}

			{
				std::lock_guard<std::mutex> guard(experimentalMutex);
				memcpy(tsExperimentalStereoImageData.get(), playground.get(), tsImageMemorySize);
			}

		}
		else
		{
			Sleep(10);
		}
	}
}

