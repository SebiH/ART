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
		bool is_first_image_ = true;


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
		virtual float GetFocalLength() const override;

		virtual nlohmann::json GetProperties() const override;
		virtual void SetProperties(const nlohmann::json &json_config) override;

	};
}
