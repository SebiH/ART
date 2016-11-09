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
	std::lock_guard<std::mutex> lock(mutex_);
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
	static bool first = true;
	std::lock_guard<std::mutex> lock(mutex_);
	if (IsOpen())
	{
		if (first)
		{
			first = false;
			ovr_camera_->SetCameraGain(40);
		}

		ovr_camera_->GetCamImageBGRA(left_buffer, OVR::Cameye::OV_CAMEYE_LEFT);
		ovr_camera_->GetCamImageBGRA(right_buffer, OVR::Cameye::OV_CAMEYE_RIGHT);
	}
}




void OvrvisionCameraSource::Open()
{
	std::lock_guard<std::mutex> lock(mutex_);
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
	std::lock_guard<std::mutex> lock(mutex_);
	if (IsOpen())
	{
		ovr_camera_->Close();
	}
}

bool OvrvisionCameraSource::IsOpen() const
{
	return ovr_camera_->isOpen();
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

float ImageProcessing::OvrvisionCameraSource::GetFocalLength() const
{
	return ovr_camera_->GetCamFocalPoint();
}


void OvrvisionCameraSource::SetProperties(const nlohmann::json &json_config)
{
	if (json_config.count("Exposure") != 0)
	{
		auto exposure = json_config["Exposure"].get<int>();
		ovr_camera_->SetCameraExposure(exposure);
	}

	if (json_config.count("ExposurePerSec") != 0)
	{
		auto fps = json_config["ExposurePerSec"].get<float>();
		ovr_camera_->SetCameraExposurePerSec(fps);
	}

	if (json_config.count("Gain") != 0)
	{
		auto gain = json_config["Gain"].get<int>();
		ovr_camera_->SetCameraGain(gain);
	}

	if (json_config.count("BLC") != 0)
	{
		auto blc = json_config["BLC"].get<int>();
		ovr_camera_->SetCameraBLC(blc);
	}

	if (json_config.count("AutoWhiteBalance") != 0)
	{
		auto whitebalance = json_config["AutoWhiteBalance"].get<bool>();
		ovr_camera_->SetCameraWhiteBalanceAuto(whitebalance);
	}

	if (json_config.count("WhiteBalanceR") != 0)
	{
		auto whitebalance = json_config["WhiteBalanceR"].get<int>();
		ovr_camera_->SetCameraWhiteBalanceR(whitebalance);
	}

	if (json_config.count("WhiteBalanceG") != 0)
	{
		auto whitebalance = json_config["WhiteBalanceG"].get<int>();
		ovr_camera_->SetCameraWhiteBalanceG(whitebalance);
	}

	if (json_config.count("WhiteBalanceB") != 0)
	{
		auto whitebalance = json_config["WhiteBalanceB"].get<int>();
		ovr_camera_->SetCameraWhiteBalanceB(whitebalance);
	}
}

nlohmann::json OvrvisionCameraSource::GetProperties() const
{
	if (IsOpen())
	{
		return nlohmann::json{
			{ "HMDRightGap", { ovr_camera_->GetHMDRightGap(0), ovr_camera_->GetHMDRightGap(1), ovr_camera_->GetHMDRightGap(2) } },
			{ "FocalPoint", ovr_camera_->GetCamFocalPoint() },
			{ "Exposure", ovr_camera_->GetCameraExposure() },
			{ "Gain", ovr_camera_->GetCameraGain() },
			{ "BLC", ovr_camera_->GetCameraBLC() },
			{ "AutoWhiteBalance", ovr_camera_->GetCameraWhiteBalanceAuto() },
			{ "WhiteBalanceR", ovr_camera_->GetCameraWhiteBalanceR() },
			{ "WhiteBalanceG", ovr_camera_->GetCameraWhiteBalanceG() },
			{ "WhiteBalanceB", ovr_camera_->GetCameraWhiteBalanceB() }
		};
	}
	else
	{
		return nlohmann::json();
	}
}

