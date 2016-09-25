#pragma once

#include <memory>
#include <mutex>
#include <ovrvision/ovrvision_pro.h>
#include "cameras/CameraSourceInterface.h"

namespace ImageProcessing
{
	class OvrvisionCameraSource : public CameraSourceInterface
	{
	private:
		std::mutex mutex_;
		std::unique_ptr<OVR::OvrvisionPro> ovr_camera_;
		OVR::Camprop quality_;
		OVR::Camqt process_mode_;

	public:
		OvrvisionCameraSource(OVR::Camprop quality, OVR::Camqt process_mode);
		~OvrvisionCameraSource();

		virtual void PrepareNextFrame() override;
		virtual void GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer) override;

		virtual void Open() override;
		virtual void Close() override;
		virtual bool IsOpen() const override;

		/*
		* Camera properties
		*/
		virtual int GetFrameWidth() const override;
		virtual int GetFrameHeight() const override;
		virtual int GetFrameChannels() const override;
		virtual float GetFocalLength() const override;

		virtual nlohmann::json GetProperties() const override;
		virtual void SetProperties(const nlohmann::json &json_config) override;
	};
}
