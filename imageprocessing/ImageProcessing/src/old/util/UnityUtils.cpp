#include "UnityUtils.h"
#include "../api/debug.h"

#include <stdio.h>

void DebugLog(const std::string &msg)
{
	// TODO: doesn't work, but is used in Unity's demo. odd.
#if UNITY_WIN
	OutputDebugStringA(str.c_str());
#else
	printf("%s\n", msg.c_str());
#endif

	// just defer to working function, until above code works in unity
	LogToUnity(msg.c_str());
}
