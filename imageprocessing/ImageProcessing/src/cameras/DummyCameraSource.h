#pragma once

#include <string>
#include <opencv2/core.hpp>
#include "cameras/CameraSourceInterface.h"

namespace ImageProcessing
{
	class DummyCameraSource : public CameraSourceInterface
	{
	private:
		cv::Mat img_;


	public:
		DummyCameraSource(std::string filename);
		~DummyCameraSource();


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
