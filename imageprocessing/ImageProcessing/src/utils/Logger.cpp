#include "utils/Logger.h"

#include <stdio.h>

static LoggerCallback g_ExternalCallback;

void DebugLog(const std::string &msg)
{
	if (g_ExternalCallback)
	{
		g_ExternalCallback(msg.c_str());
	}
	else
	{
#if UNITY_WIN
		OutputDebugStringA(str.c_str());
#else
		printf("%s\n", msg.c_str());
#endif
	}
}

void SetExternalLoggerCallback(LoggerCallback &callback)
{
	g_ExternalCallback = callback;
}
