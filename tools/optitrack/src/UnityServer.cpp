#include "UnityServer.h"

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
	server_ = std::make_unique<cSlipStream>(UnityIp.c_str(), UnityPort);
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
