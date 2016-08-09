#include "UnityUtils.h"
#include "../api/debug.h"

#include <stdio.h>

void DebugLog(const char* str)
{
	// TODO: doesn't work, but is used in Unity's demo. odd.
#if UNITY_WIN
	OutputDebugStringA(str);
#else
	printf("%s\n", str);
#endif

	// just defer to working function, until above code works in unity
	LogToUnity(str);
}
