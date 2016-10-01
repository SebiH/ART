#pragma once

#include <string>

typedef void(__stdcall * LoggerCallback) (const char *str);

void DebugLog(const std::string &msg);
void SetExternalLoggerCallback(LoggerCallback &callback);
