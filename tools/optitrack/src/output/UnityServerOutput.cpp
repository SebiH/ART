#include "output/UnityServerOutput.h"

using namespace Optitrack;

UnityServerOutput::UnityServerOutput(const std::shared_ptr<UnityServer> &server)
	: server_(server)
{

}

UnityServerOutput::~UnityServerOutput()
{

}

void UnityServerOutput::Broadcast(const char *msg)
{

}
