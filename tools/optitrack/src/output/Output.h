#pragma once

#include <string>

namespace Optitrack
{
	class Output
	{
	public:
		Output() {}
		virtual ~Output() {}
		virtual void Broadcast(const char *msg) = 0;
	};
}
