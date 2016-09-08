#pragma once

#include <memory>
#include <opencv2/videoio.hpp>
#include "cameras/CameraSourceInterface.h"

namespace ImageProcessing
{
	class OpenCVCameraSource : public CameraSourceInterface
	{
	private:
		std::unique_ptr<cv::VideoCapture> camera_;

	public:
		OpenCVCameraSource();
		~OpenCVCameraSource();

		// Inherited via CameraSourceInterface
		virtual void PrepareNextFrame() override;
		virtual void GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer) override;
		virtual void Open() override;
		virtual void Close() override;
		virtual bool IsOpen() const override;
		virtual int GetFrameWidth() const override;
		virtual int GetFrameHeight() const override;
		virtual int GetFrameChannels() const override;

		virtual nlohmann::json GetProperties() const override;
		virtual void SetProperties(const nlohmann::json &json_config) override;
	};
}
