#include "output/UnityServerOutput.h"

using namespace Optitrack;

UnityServerOutput::UnityServerOutput(const std::shared_ptr<UnityServer> &server)
	: server_(server)
{

}

UnityServerOutput::~UnityServerOutput()
{

}

void UnityServerOutput::Broadcast(const std::string &msg)
{
	if (server_->IsRunning())
	{
		server_->Send((unsigned char *)msg.c_str(), msg.length());
	}
}
