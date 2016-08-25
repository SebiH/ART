#include <memory>
#include <ovrvision/ovrvision_pro.h>
#include <Unity/IUnityInterface.h>

#include "cameras/ActiveCamera.h"
#include "cameras/CameraSourceInterface.h"
#include "cameras/OvrvisionCameraSource.h"

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
	active_camera->SetSource(std::move(new_camera));
}


extern "C" UNITY_INTERFACE_EXPORT void SetOvrCamera(const int resolution, const int processing_mode)
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
		// TODO.
	}
}


