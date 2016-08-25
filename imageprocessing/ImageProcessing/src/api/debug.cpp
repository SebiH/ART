#include <Unity/IUnityInterface.h>
#include "utils/Logger.h"

// See: http://answers.unity3d.com/questions/30620/how-to-debug-c-dll-code.html

extern "C" UNITY_INTERFACE_EXPORT void RegisterLoggerCallback(LoggerCallback callback)
{
	if (callback)
	{
		SetExternalLoggerCallback(callback);
	}
}
