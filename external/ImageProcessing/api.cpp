#pragma once

#define DllExport   __declspec( dllexport )

#include <d3d11.h>
#include <opencv2/opencv.hpp>
#include <ovrvision_pro.h>

using namespace cv;

OVR::OvrvisionPro ovrCamera;
int camWidth, camHeight;

extern "C" DllExport void OvrStart(int cameraMode = -1)
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


extern "C" DllExport void OvrStop()
{
	if (ovrCamera.isOpen())
	{
		ovrCamera.Close();
	}
}


extern "C" DllExport float GetProperty(const char *name)
{
	std::string prop(name);

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
	else if (prop == "isOpen")
	{
		return (ovrCamera.isOpen()) ? 10.0f : 0.0f; // TODO: better return value?
	}
	else
	{
		cv::Mat test(cv::Size(200, 500), CV_64F);
		imshow("testwin", test);
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

static float g_Time = 0.0f;

static void FillTextureFromCode(int width, int height, int stride, unsigned char* dst)
{
	const float t = g_Time * 4.0f;
	g_Time += 0.1f;

	for (int y = 0; y < height; ++y)
	{
		unsigned char* ptr = dst;
		for (int x = 0; x < width; ++x)
		{
			// Simple oldskool "plasma effect", a bunch of combined sine waves
			int vv = int(
				(127.0f + (127.0f * sinf(x / 7.0f + t))) +
				(127.0f + (127.0f * sinf(y / 5.0f - t))) +
				(127.0f + (127.0f * sinf((x + y) / 6.0f - t))) +
				(127.0f + (127.0f * sinf(sqrtf(float(x*x + y*y)) / 4.0f - t)))
				) / 4;

			// Write the texture pixel
			ptr[0] = vv;
			ptr[1] = vv;
			ptr[2] = vv;
			ptr[3] = vv;

			// To next pixel (our pixels are 4 bpp)
			ptr += 4;
		}

		// To next image row
		dst += stride;
	}
}

extern "C" DllExport void WriteTexture(unsigned char *leftUnityPtr, unsigned char *rightUnityPtr)
{
	if (ovrCamera.isOpen())
	{
		ovrCamera.PreStoreCamData(OVR::OV_CAMQT_DMS);

		unsigned char *leftImg = ovrCamera.GetCamImageBGRA(OVR::OV_CAMEYE_LEFT);
		unsigned char *rightImg = ovrCamera.GetCamImageBGRA(OVR::OV_CAMEYE_RIGHT);

		ID3D11Texture2D* d3dtex = (ID3D11Texture2D*)leftUnityPtr;
		ID3D11Device *g_D3D11Device;
		d3dtex->GetDevice(&g_D3D11Device);

		float phi = g_Time;
		float cosPhi = cosf(phi);
		float sinPhi = sinf(phi);

		float worldMatrix[16] = {
			cosPhi,-sinPhi,0,0,
			sinPhi,cosPhi,0,0,
			0,0,1,0,
			0,0,0.7f,1,
		};

		ID3D11DeviceContext* ctx = NULL;
		g_D3D11Device->GetImmediateContext(&ctx);

		//// update constant buffer - just the world matrix in our case
		//ctx->UpdateSubresource(g_D3D11CB, 0, NULL, worldMatrix, 64, 0);

		//// set shaders
		//ctx->VSSetConstantBuffers(0, 1, &g_D3D11CB);
		//ctx->VSSetShader(g_D3D11VertexShader, NULL, 0);
		//ctx->PSSetShader(g_D3D11PixelShader, NULL, 0);

		//// update vertex buffer
		//ctx->UpdateSubresource(g_D3D11VB, 0, NULL, verts, sizeof(verts[0]) * 3, 0);

		//// set input assembler data and draw
		//ctx->IASetInputLayout(g_D3D11InputLayout);
		//ctx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
		//UINT stride = sizeof(MyVertex);
		//UINT offset = 0;
		//ctx->IASetVertexBuffers(0, 1, &g_D3D11VB, &stride, &offset);
		//ctx->Draw(3, 0);

		// update native texture from code
		//if (g_TexturePointer)
		{
			//ID3D11Texture2D* d3dtex = (ID3D11Texture2D*)g_TexturePointer;
			D3D11_TEXTURE2D_DESC desc;
			d3dtex->GetDesc(&desc);

			//unsigned char* data = new unsigned char[desc.Width*desc.Height * 4];
			//FillTextureFromCode(desc.Width, desc.Height, desc.Width * 4, data);
			//ctx->UpdateSubresource(d3dtex, 0, NULL, data, desc.Width * 4, 0);

			//delete[] data;



			ctx->UpdateSubresource(d3dtex, 0, NULL, leftImg, desc.Width * 4, 0);
			cv::Mat mat(camHeight, camWidth, CV_8UC4, leftImg);
			imshow("xyz", mat);
			cv::waitKey(10);
		}

		ctx->Release();

	}
}
