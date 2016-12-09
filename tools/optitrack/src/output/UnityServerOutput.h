#pragma once

#include <memory>
#include "output/Output.h"
#include "UnityServer.h"

namespace Optitrack
{
	class UnityServerOutput : public Output
	{
	public:
		UnityServerOutput(const std::shared_ptr<UnityServer> &server);
		~UnityServerOutput();
		void Broadcast(const std::string &msg);

	private:
		std::shared_ptr<UnityServer> server_;
	};
}
