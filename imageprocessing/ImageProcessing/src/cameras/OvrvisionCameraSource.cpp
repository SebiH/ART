#include "cameras/OvrvisionCameraSource.h"

using namespace ImageProcessing;

OvrvisionCameraSource::OvrvisionCameraSource(OVR::Camprop quality, OVR::Camqt process_mode)
	: ovr_camera_(std::make_unique<OVR::OvrvisionPro>()),
	  quality_(quality),
	  process_mode_(process_mode)
{
	ovr_camera_->SetCameraSyncMode(true);
}


OvrvisionCameraSource::~OvrvisionCameraSource()
{
	Close();
}



void OvrvisionCameraSource::PrepareNextFrame()
{
	std::unique_lock<std::mutex> lock(mutex_);
	if (IsOpen())
	{
		ovr_camera_->PreStoreCamData(process_mode_);
	}
	else
	{
		throw std::exception("Camera is closed");
	}
}


void OvrvisionCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
	std::unique_lock<std::mutex> lock(mutex_);
	if (IsOpen())
	{
		ovr_camera_->GetCamImageBGRA(left_buffer, OVR::Cameye::OV_CAMEYE_LEFT);
		ovr_camera_->GetCamImageBGRA(right_buffer, OVR::Cameye::OV_CAMEYE_RIGHT);
	}
}




void OvrvisionCameraSource::Open()
{
	std::unique_lock<std::mutex> lock(mutex_);
	if (!IsOpen())
	{
		auto open_success = ovr_camera_->Open(0, quality_);

		if (!open_success)
		{
			throw std::exception("Could not open OVRvision camera");
		}
	}
}

void OvrvisionCameraSource::Close()
{
	std::unique_lock<std::mutex> lock(mutex_);
	if (IsOpen())
	{
		ovr_camera_->Close();
	}
}

bool OvrvisionCameraSource::IsOpen() const
{
	return ovr_camera_->isOpen();
}





float OvrvisionCameraSource::GetCamFocalPoint() const
{
	return ovr_camera_->GetCamFocalPoint();
}


float OvrvisionCameraSource::GetHMDRightGap(const int at) const
{
	return ovr_camera_->GetHMDRightGap(at);
}

void OvrvisionCameraSource::SetProcessingMode(const OVR::Camqt mode)
{
	process_mode_ = mode;
}

int OvrvisionCameraSource::GetProcessingMode() const
{
	return process_mode_;
}




int OvrvisionCameraSource::GetFrameWidth() const
{
	return ovr_camera_->GetCamWidth();
}

int OvrvisionCameraSource::GetFrameHeight() const
{
	return ovr_camera_->GetCamHeight();
}

int OvrvisionCameraSource::GetFrameChannels() const
{
	//return ovr_camera_->GetCamPixelsize();
	return 4;
}


void OvrvisionCameraSource::SetProperties(const nlohmann::json json_config)
{

}

nlohmann::json OvrvisionCameraSource::GetProperties() const
{
	return nlohmann::json();
}

