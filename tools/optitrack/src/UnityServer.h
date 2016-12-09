#pragma once

#include <memory>
#include <string>
#include <NatNet/NatNetTypes.h>
#include <NatNet/NatNetRepeater.h>

namespace Optitrack
{
	class UnityServer
	{
	public:
		UnityServer();
		~UnityServer();

		void Start();
		void Stop();

		bool IsRunning() const;
		void Send(unsigned char *msg, int length);

		std::string UnityIp = std::string("127.0.0.1");
		int UnityPort = 16000;

	private:
		bool is_running_ = false;
		std::unique_ptr<cSlipStream> server_;
	};
}
