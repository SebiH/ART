#pragma once

#include <map>
#include <memory>
#include <string>
#include <NatNet/NatNetClient.h>
#include <NatNet/NatNetTypes.h>
#include "input/Input.h"

namespace Optitrack
{
	class OptitrackInput : public Input
	{
	public:
		OptitrackInput() {}
		virtual ~OptitrackInput();

		// see static functions below
		static OptitrackInput * Instance()
		{
			static OptitrackInput *instance = new OptitrackInput();
			return instance;
		}

		void Start();
		void Stop();
		bool IsRunning() const { return is_running_; }

		std::string OptitrackIp;
		std::string LocalIp;

		int DataPort = 3130;
		int CommandPort = 3131;

		// ConnectionType_Multicast || ConnectionType_Unicast
		int ConnectionType = ConnectionType_Multicast;

	private:
		bool is_running_;
		std::unique_ptr<NatNetClient> client_;
		// workarounds because natnetclient needs c strings for ips + memory?
		std::unique_ptr<char[]> local_ip_, optitrack_ip_;
		// track rigid body names
		std::map<int, std::string> bone_names_;

		void CreateClient();
		void InitClient();

		// need to have static functions (and therefore instance)
		// due to optitrack client only accepting function pointers

		static void DataHandler(sFrameOfMocapData *motionData, void *userData);
		static void ErrorHandler(int msgType, char *msg);
	};
}
