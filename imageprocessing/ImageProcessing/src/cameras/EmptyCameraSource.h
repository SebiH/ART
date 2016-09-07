#pragma once

#include "cameras/CameraSourceInterface.h"

namespace ImageProcessing
{
	class EmptyCameraSource : public CameraSourceInterface
	{
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
