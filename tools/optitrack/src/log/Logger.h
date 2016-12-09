#pragma once

namespace Optitrack
{
	class Logger
	{
	public:
		void Write(const char *msg);
		void WriteLine(const char *msg);
	};
}
