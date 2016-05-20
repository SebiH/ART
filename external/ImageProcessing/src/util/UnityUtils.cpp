#include "UnityUtils.h"

#include <Unity/IUnityInterface.h>
#include <Unity/IUnityGraphics.h>

#include <stdio.h>

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

void DebugInUnity(std::string message)
{
	if (gDebugCallback)
	{
		gDebugCallback(message.c_str());
	}
}

void DebugLog(const char* str)
{
	// TODO: doesn't work, but is used in Unity's demo. odd.
#if UNITY_WIN
	OutputDebugStringA(str);
#else
	printf("%s", str);
#endif

	// just defer to working function, until above code works in unity
	DebugInUnity(std::string(str));
}
