#include <memory>
#include <ovrvision\ovrvision_pro.h>
#include <Unity\IUnityInterface.h>

#include "cameras\ActiveCamera.h"
#include "cameras\ICameraSource.h"
#include "cameras\OvrvisionCameraSource.h"

static void SetCamera(std::shared_ptr<ImageProcessing::ICameraSource> &newCamera)
{
	auto activeCamera = ImageProcessing::ActiveCamera::Instance();
	auto camSrc = activeCamera->GetSource();

	// auto close old cam (if exists)
	if (camSrc.get() != nullptr && camSrc->IsOpen())
	{
		camSrc->Close();
	}

	// auto open
	if (!newCamera->IsOpen())
	{
		newCamera->Open();
	}

	// transfer ownership
	activeCamera->SetSource(std::move(newCamera));
}


extern "C" UNITY_INTERFACE_EXPORT void SetOvrCamera(const int resolution, const int processingMode)
{
	auto cameraResolution = static_cast<OVR::Camprop>(resolution);
	auto cameraMode = static_cast<OVR::Camqt>(processingMode);

	try
	{
		std::shared_ptr<ImageProcessing::ICameraSource> ovrSource = std::make_shared<ImageProcessing::OvrvisionCameraSource>(cameraResolution, cameraMode);
		SetCamera(ovrSource);
	}
	catch (const std::exception &e)
	{
	}
}


