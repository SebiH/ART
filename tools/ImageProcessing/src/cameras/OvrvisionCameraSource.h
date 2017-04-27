#pragma once

#include <memory>
#include <mutex>
#include <ovrvision/ovrvision_pro.h>
#include <opencv2/opencv.hpp>
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

		bool use_auto_contrast_ = true;
		bool auto_contrast_auto_gain_ = true;
		float auto_contrast_clip_percent_ = 0;
		float auto_contrast_max_ = 7;

		float avg_alpha = 1;
		float avg_beta = 1;

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

	private:
		void BrightnessAndContrastAuto(cv::Mat &left, cv::Mat &right, float clipHistPercent = 0);
	};
}
