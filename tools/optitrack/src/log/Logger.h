#pragma once

namespace Optitrack
{

	typedef void(__stdcall * LoggerCallback) (const char *str);

	class Logger
	{
	public:
		Logger() {};
		~Logger() {};

		void Write(const char *msg);
		void WriteLine(const char *msg);

		void SetExternalLoggerCallback(LoggerCallback &callback);

	private:
		LoggerCallback callback_ = nullptr;
	};
}
