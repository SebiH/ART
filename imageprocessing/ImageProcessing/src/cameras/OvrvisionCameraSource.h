#pragma once

#include <memory>
#include <ovrvision\ovrvision_pro.h>
#include "cameras/ICameraSource.h"

namespace ImageProcessing
{
	class OvrvisionCameraSource : public ICameraSource
	{
	private:


	public:
		OvrvisionCameraSource(OVR::Camprop quality, OVR::Camqt processMode);
		~OvrvisionCameraSource();

		virtual ImageProcessing::ImageMetaData GetCurrentFrame() override;

		virtual bool IsOpen() const override;
		virtual void Open() override;
		virtual void Close() override;


		/*
		 * Ovrvision specific properties
		 */
		float GetCamFocalPoint() const;
		float GetHMDRightGap(const int at) const;
		void SetProcessingMode(const OVR::Camqt mode);
		int GetProcessingMode() const;

		/*
		 * Camera properties
		 */
		virtual int GetFrameWidth() const override;
		virtual int GetFrameHeight() const override;
		virtual int GetFrameChannels() const override;
		virtual int GetCamExposure() const override;
		virtual void SetCamExposure(const int val) const override;
		virtual int GetCamGain() const override;
		virtual void SetCamGain(const int val) const override;
		virtual int GetCamBLC() const override;
		virtual void SetCamBLC(const int val) const override;
		virtual bool GetCamAutoWhiteBalance() const override;
		virtual void SetCamAutoWhiteBalance(const bool val) const override;
		virtual int GetCamWhiteBalanceR() const override;
		virtual void SetCamWhiteBalanceR(const int val) const override;
		virtual int GetCamWhiteBalanceG() const override;
		virtual void SetCamWhiteBalanceG(const int val) const override;
		virtual int GetCamWhiteBalanceB() const override;
		virtual void SetCamWhiteBalanceB(const int val) const override;
		virtual int GetCamFps() const override;
	};
}
