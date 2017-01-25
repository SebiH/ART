#include "log/Log.h"

#include <stdarg.h>

using namespace Optitrack;

void Log::Init(int log_level, LoggerCallback callback)
{
	Instance()->log_level_ = log_level;
	Instance()->logger_ = std::make_unique<Logger>();
	Instance()->logger_->SetExternalLoggerCallback(callback);
}


void Log::Debug(const std::string &fmt_str, ...)
{
	if (Instance()->log_level_ == 0)
	{
		return;
	}

	if (fmt_str.length() == 0)
	{
		Instance()->logger_->WriteLine("");
		return;
	}


	// Taken from http://stackoverflow.com/a/8098080/4090817
	int final_n, n = ((int)fmt_str.size()) * 2; /* Reserve two times as much as the length() of the fmt_str */
	std::string str;
	std::unique_ptr<char[]> formatted;
	va_list ap;
	while (1)
	{
		formatted.reset(new char[n]); /* Wrap the plain char array into the unique_ptr */
		strcpy(&formatted[0], fmt_str.c_str());
		va_start(ap, fmt_str);
		final_n = vsnprintf(&formatted[0], n, fmt_str.c_str(), ap);
		va_end(ap);
		if (final_n < 0 || final_n >= n)
			n += abs(final_n - n + 1);
		else
			break;
	}

	Instance()->logger_->Write("[DEBUG] ");
	Instance()->logger_->Write(formatted.get());
	Instance()->logger_->Write("\n");
}

void Log::Info(const std::string &fmt_str, ...)
{
	if (fmt_str.length() == 0)
	{
		Instance()->logger_->WriteLine("");
		return;
	}

	// Taken from http://stackoverflow.com/a/8098080/4090817
	int final_n, n = ((int)fmt_str.size()) * 2; /* Reserve two times as much as the length() of the fmt_str */
	std::string str;
	std::unique_ptr<char[]> formatted;
	va_list ap;
	while (1)
	{
		formatted.reset(new char[n]); /* Wrap the plain char array into the unique_ptr */
		strcpy(&formatted[0], fmt_str.c_str());
		va_start(ap, fmt_str);
		final_n = vsnprintf(&formatted[0], n, fmt_str.c_str(), ap);
		va_end(ap);
		if (final_n < 0 || final_n >= n)
			n += abs(final_n - n + 1);
		else
			break;
	}

	Instance()->logger_->WriteLine(formatted.get());
}

void Log::Warning(const std::string &fmt_str, ...)
{
	if (fmt_str.length() == 0)
	{
		Instance()->logger_->WriteLine("");
		return;
	}

	// Taken from http://stackoverflow.com/a/8098080/4090817
	int final_n, n = ((int)fmt_str.size()) * 2; /* Reserve two times as much as the length() of the fmt_str */
	std::string str;
	std::unique_ptr<char[]> formatted;
	va_list ap;
	while (1)
	{
		formatted.reset(new char[n]); /* Wrap the plain char array into the unique_ptr */
		strcpy(&formatted[0], fmt_str.c_str());
		va_start(ap, fmt_str);
		final_n = vsnprintf(&formatted[0], n, fmt_str.c_str(), ap);
		va_end(ap);
		if (final_n < 0 || final_n >= n)
			n += abs(final_n - n + 1);
		else
			break;
	}

	Instance()->logger_->Write("[WARNING] ");
	Instance()->logger_->Write(formatted.get());
	Instance()->logger_->Write("\n");
}

void Log::Error(const std::string &fmt_str, ...)
{
	if (fmt_str.length() == 0)
	{
		Instance()->logger_->WriteLine("");
		return;
	}

	// Taken from http://stackoverflow.com/a/8098080/4090817
	int final_n, n = ((int)fmt_str.size()) * 2; /* Reserve two times as much as the length() of the fmt_str */
	std::string str;
	std::unique_ptr<char[]> formatted;
	va_list ap;
	while (1)
	{
		formatted.reset(new char[n]); /* Wrap the plain char array into the unique_ptr */
		strcpy(&formatted[0], fmt_str.c_str());
		va_start(ap, fmt_str);
		final_n = vsnprintf(&formatted[0], n, fmt_str.c_str(), ap);
		va_end(ap);
		if (final_n < 0 || final_n >= n)
			n += abs(final_n - n + 1);
		else
			break;
	}

	Instance()->logger_->Write("[ERROR] ");
	Instance()->logger_->Write(formatted.get());
	Instance()->logger_->Write("\n");
}

