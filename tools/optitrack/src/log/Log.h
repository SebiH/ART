#pragma once

#include <memory>
#include <string>
#include "log/Logger.h"

namespace Optitrack
{
	class Log
	{
	public:
		static void Debug(const std::string &msg);
		static void Info(const std::string &msg);
		static void Warning(const std::string &msg);
		static void Error(const std::string &msg);

		static std::string Format(const std::string &format, ...);

		static void Init(int log_level, LoggerCallback callback);

	private:
		static Log * Instance()
		{
			static Log *instance = new Log();
			return instance;
		}

		Log() : logger_(std::make_unique<Logger>()), log_level_(0) {}
		~Log() {};

		std::unique_ptr<Logger> logger_;
		int log_level_;
	};
}
