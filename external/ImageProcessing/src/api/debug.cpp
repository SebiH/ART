#include "debug.h"

#include <Unity\IUnityInterface.h>

// See: http://answers.unity3d.com/questions/30620/how-to-debug-c-dll-code.html
typedef void(__stdcall * DebugCallback) (const char *str);
DebugCallback gDebugCallback;

extern "C" UNITY_INTERFACE_EXPORT void RegisterDebugCallback(DebugCallback callback)
{
	if (callback)
	{
		gDebugCallback = callback;
	}
}

void LogToUnity(const char *msg)
{
	if (gDebugCallback)
	{
		gDebugCallback(msg);
	}
}
