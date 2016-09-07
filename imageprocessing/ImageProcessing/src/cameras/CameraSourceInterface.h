#pragma once

#include <string>
#include <json/json.hpp>

namespace ImageProcessing
{
	class CameraSourceInterface
	{
	public:
		virtual ~CameraSourceInterface() {}

		virtual void PrepareNextFrame() = 0;
		virtual void GrabFrame(unsigned char *left_buffer, unsigned char *right_buffer) = 0;

		virtual void Open() = 0;
		virtual void Close() = 0;
		virtual bool IsOpen() const = 0;

		// Camera Properties
		virtual int GetFrameWidth() const = 0;
		virtual int GetFrameHeight() const = 0;
		virtual int GetFrameChannels() const = 0;

		virtual nlohmann::json GetProperties() const = 0;
		virtual void SetProperties(const nlohmann::json &json_config) = 0;
	};
}
