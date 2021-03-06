#include <Unity/IUnityInterface.h>
#include <json/json.hpp>

#include "cameras/ActiveCamera.h"
#include "cameras/CameraSourceInterface.h"
#include "utils/Logger.h"

extern "C" UNITY_INTERFACE_EXPORT int GetCamWidth()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		return cam->GetFrameWidth();
	}
	else
	{
		return 0;
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamHeight()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();

	if (cam)
	{
		return cam->GetFrameHeight();
	}
	else
	{
		return 0;
	}
}

extern "C" UNITY_INTERFACE_EXPORT int GetCamChannels()
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam.get() != nullptr)
	{
		return cam->GetFrameChannels();
	}
	else
	{
		return 0;
	}
}

// workaround since strings can't easily be returned to c#
typedef void(__stdcall * PropertyCallback) (const char *str);
extern "C" UNITY_INTERFACE_EXPORT void GetCamJsonProperties(PropertyCallback callback)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();

	if (cam)
	{
		auto props = cam->GetProperties();
		callback(props.dump(4).c_str());
	}
}

extern "C" UNITY_INTERFACE_EXPORT void SetCamJsonProperties(const char * json_str_config)
{
	auto cam = ImageProcessing::ActiveCamera::Instance()->GetSource();
	if (cam)
	{
		try
		{
			auto json_config = nlohmann::json::parse(json_str_config);
			cam->SetProperties(json_config);
		}
		catch (const std::exception &e)
		{
			DebugLog(std::string("Invalid json: ") + e.what());
		}
	}
}
