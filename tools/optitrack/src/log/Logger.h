#pragma once

namespace Optitrack
{

	typedef void(__stdcall * LoggerCallback) (const char *str);

	class Logger
	{
	public:
		void Write(const char *msg);
		void WriteLine(const char *msg);

		void SetExternalLoggerCallback(LoggerCallback &callback);

	private:
		LoggerCallback callback_;
	};
}
