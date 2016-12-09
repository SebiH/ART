#pragma once

#include <string>

namespace Optitrack
{
	class Output
	{
	public:
		Output();
		virtual ~Output() {}

		int Id;

		virtual void Broadcast(const std::string &msg) = 0;
	};
}
