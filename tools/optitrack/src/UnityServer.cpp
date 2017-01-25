#include "UnityServer.h"
#include "log/Log.h"

using namespace Optitrack;

UnityServer::UnityServer()
{

}

UnityServer::~UnityServer()
{
	Stop();
}

void UnityServer::Start()
{
	is_running_ = true;

	if (UnityIp.length() == 0)
	{
		UnityIp = "127.0.0.1";
	}

	server_ = std::make_unique<cSlipStream>(UnityIp.c_str(), UnityPort);
	Log::Info(Log::Format("Starting unity server on %s:%d", UnityIp.c_str(), UnityPort));
}

void UnityServer::Stop()
{
	is_running_ = false;
}

bool UnityServer::IsRunning() const
{
	return is_running_;
}

void UnityServer::Send(unsigned char *msg, int length)
{
	server_->Stream(msg, length);
}
