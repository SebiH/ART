#include <memory>
#include <ovrvision/ovrvision_pro.h>
#include <Unity/IUnityInterface.h>

#include "cameras/ActiveCamera.h"
#include "cameras/CameraSourceInterface.h"
#include "cameras/EmptyCameraSource.h"
#include "cameras/DummyCameraSource.h"
#include "cameras/OvrvisionCameraSource.h"
#include "cameras/OpenCVCameraSource.h"
#include "utils/Logger.h"


extern "C" UNITY_INTERFACE_EXPORT void StartImageProcessing()
{
	ImageProcessing::ActiveCamera::Instance()->Start();
}

extern "C" UNITY_INTERFACE_EXPORT void StopImageProcessing()
{
	ImageProcessing::ActiveCamera::Instance()->Stop();
}


static void SetCamera(std::shared_ptr<ImageProcessing::CameraSourceInterface> &new_camera)
{
	auto active_camera = ImageProcessing::ActiveCamera::Instance();
	auto cam_src = active_camera->GetSource();

	// auto close old cam (if exists)
	if (cam_src.get() != nullptr && cam_src->IsOpen())
	{
		cam_src->Close();
	}

	// auto open
	if (!new_camera->IsOpen())
	{
		new_camera->Open();
	}

	// transfer ownership
	active_camera->SetActiveSource(std::move(new_camera));
}


extern "C" UNITY_INTERFACE_EXPORT void SetOvrCamera(const int /* OVR::Camprop */ resolution, const int /* OVR::Camqt */ processing_mode)
{
	auto cam_resolution = static_cast<OVR::Camprop>(resolution);
	auto cam_mode = static_cast<OVR::Camqt>(processing_mode);

	try
	{
		std::shared_ptr<ImageProcessing::CameraSourceInterface> ovr_source = std::make_shared<ImageProcessing::OvrvisionCameraSource>(cam_resolution, cam_mode);
		SetCamera(ovr_source);
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to set OVR Camera: ") + e.what());
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetDummyCamera(const char *filepath)
{
	try
	{
		std::shared_ptr<ImageProcessing::CameraSourceInterface> dummy_source = std::make_shared<ImageProcessing::DummyCameraSource>(filepath);
		SetCamera(dummy_source);
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to set dummy source: ") + e.what());
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetOpenCVCamera()
{
	try
	{
		std::shared_ptr<ImageProcessing::CameraSourceInterface> opencv_source = std::make_shared<ImageProcessing::OpenCVCameraSource>();
		SetCamera(opencv_source);
	}
	catch (const std::exception &e)
	{
		DebugLog(std::string("Unable to set dummy source: ") + e.what());
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetEmptyCamera()
{
	std::shared_ptr<ImageProcessing::CameraSourceInterface> empty_source = std::make_shared<ImageProcessing::EmptyCameraSource>();
	SetCamera(empty_source);
}

