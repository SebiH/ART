#include "log/Logger.h"

using namespace Optitrack;

void Logger::Write(const char *msg)
{
	if (callback_)
	{
		callback_(msg);
	}
}

void Logger::WriteLine(const char *msg)
{
	if (callback_)
	{
		callback_(msg);
		callback_("\n");
	}
}

void Optitrack::Logger::SetExternalLoggerCallback(LoggerCallback &callback)
{
	callback_ = callback;
}
