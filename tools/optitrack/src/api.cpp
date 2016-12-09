#include "input/OptitrackInput.h"
#include "log/Log.h"
#include "output/UnityServerOutput.h"

#define DLL_EXPORT extern "C" __declspec(dllexport)

using namespace Optitrack;

DLL_EXPORT void SetLogger(int log_level, LoggerCallback callback)
{
	Log::Init(log_level, callback);
}

DLL_EXPORT void StartOptitrackServer(const char *optitrack_ip, int data_port, int command_port, const char *local_ip)
{
	auto server = OptitrackInput::Instance();

	if (!server->IsRunning())
	{
		server->CommandPort = command_port;
		server->DataPort = data_port;
		server->OptitrackIp = std::string(optitrack_ip);
		server->LocalIp = std::string(local_ip);
		
		server->Start();
	}
	else
	{
		Log::Error("Server is already running");
	}
}

DLL_EXPORT void StopOptitrackServer()
{
	auto server = OptitrackInput::Instance();

	if (server->IsRunning())
	{
		server->Stop();
	}
	else
	{
		Log::Error("Server is not running");
	}
}


static std::shared_ptr<UnityServer> unity_server_;

DLL_EXPORT void AttachUnityOutput(const char *unity_ip, const int port)
{
	if (!unity_server_)
	{
		unity_server_ = std::make_shared<UnityServer>();
		unity_server_->UnityIp = std::string(unity_ip);
		unity_server_->UnityPort = port;
		unity_server_->Start();

		auto unity_output = std::make_shared<UnityServerOutput>(unity_server_);
		OptitrackInput::Instance()->AddOutput(unity_output);
	}
	else
	{
		Log::Warning("UnityServer already running, unable to set ip/port");
	}
}
