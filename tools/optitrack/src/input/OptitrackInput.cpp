#include "input/OptitrackInput.h"

#include "log/Log.h"
#include "message/JsonOptitrackMessage.h"

using namespace Optitrack;

OptitrackInput::~OptitrackInput()
{
	Stop();
}

bool OptitrackInput::Start()
{
	if (is_running_)
	{
		Log::Warning("Optitrack Listener already running!");
		return false;
	}

	try
	{
		is_running_ = true;
		Log::Info("Starting Optitrack Listener listening on %s for multicast from %s", LocalIp.c_str(), OptitrackIp.c_str());
		CreateClient();
		InitClient();
		return true;
	}
	catch (const std::exception &e)
	{
		Log::Error(e.what());
		Stop();
		return false;
	}

}

void OptitrackInput::Stop()
{
	if (client_)
	{
		client_->Uninitialize();
		client_.release();
		local_ip_.release();
		optitrack_ip_.release();
		bone_names_ = std::map<int, std::string>();

		is_running_ = false;
	}

}

void OptitrackInput::CreateClient()
{
	client_ = std::make_unique<NatNetClient>(ConnectionType);

	unsigned char ver[4];
	client_->NatNetVersion(ver);
	Log::Info("NatNet Sample Client (NatNet ver. %d.%d.%d.%d)", ver[0], ver[1], ver[2], ver[3]);

	client_->SetMessageCallback(OptitrackInput::ErrorHandler);
	client_->SetVerbosityLevel(Verbosity_Error);
	client_->SetDataCallback(OptitrackInput::DataHandler, client_.get());

	local_ip_ = std::unique_ptr<char[]>(new char[LocalIp.length() + 1]);
	strncpy(local_ip_.get(), LocalIp.c_str(), LocalIp.length() + 1);

	optitrack_ip_ = std::unique_ptr<char[]>(new char[OptitrackIp.length() + 1]);
	strncpy(optitrack_ip_.get(), OptitrackIp.c_str(), OptitrackIp.length() + 1);

	int ret_code = client_->Initialize(local_ip_.get(), optitrack_ip_.get());
	if (ret_code != ErrorCode_OK)
	{
		throw std::exception((std::string("Unable to connect to server.  Error code: ") + std::to_string(ret_code)).c_str());
	}

	// print server info
	sServerDescription srv_description;
	memset(&srv_description, 0, sizeof(srv_description));
	client_->GetServerDescription(&srv_description);

	if (!srv_description.HostPresent)
	{
		throw std::exception("Unable to connect to server: Host not present");
	}

	Log::Info("[SampleClient] Server application info:");
	Log::Info("Application: %s (ver. %d.%d.%d.%d)", srv_description.szHostApp, srv_description.HostAppVersion[0],
		srv_description.HostAppVersion[1], srv_description.HostAppVersion[2], srv_description.HostAppVersion[3]);
	Log::Info("NatNet Version: %d.%d.%d.%d", srv_description.NatNetVersion[0], srv_description.NatNetVersion[1],
		srv_description.NatNetVersion[2], srv_description.NatNetVersion[3]);
	Log::Info("Local IP:%s", LocalIp.c_str());
	Log::Info("Optitrack IP:%s", OptitrackIp.c_str());
	Log::Info("Server Name:%s", srv_description.szHostComputerName);
	Log::Info(""); // empty line
}

void OptitrackInput::InitClient()
{
	// send/receive test request
	Log::Info("Sending Test Request");
	void* response;
	int num_bytes;
	auto success = client_->SendMessageAndWait("TestRequest", &response, &num_bytes);
	if (success == ErrorCode_OK)
	{
		Log::Info("Received: %s", (char*)response);
	}

	// Retrieve Data Descriptions from server
	Log::Info("Requesting Data Descriptions...");
	sDataDescriptions* data_refs = NULL;
	int nBodies = client_->GetDataDescriptions(&data_refs);

	if (!data_refs)
	{
		throw std::exception("Unable to retrieve Data Descriptions.");
	}

	Log::Info("Received %d Data Descriptions:", data_refs->nDataDescriptions);

	for (int i = 0; i < data_refs->nDataDescriptions; i++)
	{
		Log::Debug("Data Description # %d (type=%d)", i, data_refs->arrDataDescriptions[i].type);

		if (data_refs->arrDataDescriptions[i].type == Descriptor_MarkerSet)
		{
			// MarkerSet
			sMarkerSetDescription* pMS = data_refs->arrDataDescriptions[i].Data.MarkerSetDescription;
			Log::Debug("MarkerSet Name : %s", pMS->szName);
			for (int i = 0; i < pMS->nMarkers; i++)
			{
				Log::Debug("%s", pMS->szMarkerNames[i]);
			}

		}
		else if (data_refs->arrDataDescriptions[i].type == Descriptor_RigidBody)
		{
			// RigidBody
			sRigidBodyDescription* pRB = data_refs->arrDataDescriptions[i].Data.RigidBodyDescription;
			Log::Info( "RigidBody Name : %s", pRB->szName);
			Log::Debug("RigidBody ID : %d", pRB->ID);
			Log::Debug("RigidBody Parent ID : %d", pRB->parentID);
			Log::Debug("Parent Offset : %3.2f,%3.2f,%3.2f", pRB->offsetx, pRB->offsety, pRB->offsetz);
			bone_names_[pRB->ID] = pRB->szName;
		}
		else if (data_refs->arrDataDescriptions[i].type == Descriptor_Skeleton)
		{
			// Skeleton
			sSkeletonDescription* pSK = data_refs->arrDataDescriptions[i].Data.SkeletonDescription;
			Log::Info("Skeleton Name : %s", pSK->szName);
			Log::Debug("Skeleton ID : %d", pSK->skeletonID);
			Log::Debug("RigidBody (Bone) Count : %d", pSK->nRigidBodies);

			for (int j = 0; j < pSK->nRigidBodies; j++)
			{
				sRigidBodyDescription* pRB = &pSK->RigidBodies[j];
				Log::Info( "  RigidBody Name : %s", pRB->szName);
				Log::Debug("  RigidBody ID : %d", pRB->ID);
				Log::Debug("  RigidBody Parent ID : %d", pRB->parentID);
				Log::Debug("  Parent Offset : %3.2f,%3.2f,%3.2f", pRB->offsetx, pRB->offsety, pRB->offsetz);

				bone_names_[pRB->ID] = pRB->szName;
			}
		}
		else
		{
			Log::Warning("Unknown data type");
			// Unknown
		}
	}

}

void OptitrackInput::DataHandler(sFrameOfMocapData *motion_data, void *user_data)
{
	NatNetClient* pClient = (NatNetClient*)user_data;
	Log::Debug("Received frame %d", motion_data->iFrame);

	if (motion_data->nRigidBodies > 0 || motion_data->nSkeletons > 0)
	{
		JsonOptitrackMessage msg;

		// TODO: skeletons?
		for (const auto &rb_data : motion_data->RigidBodies)
		{
			Vector3 position(rb_data.x, rb_data.y, rb_data.z);
			Quaternion rotation(rb_data.qx, rb_data.qy, rb_data.qz, rb_data.qw);
			auto name = Instance()->bone_names_[rb_data.ID];

			Rigidbody rigidbody(rb_data.ID, name, position, rotation);

			for (int marker_index; marker_index < rb_data.nMarkers; marker_index++)
			{
				const auto marker_data = rb_data.Markers[marker_index];
				auto marker_id = rb_data.MarkerIDs[marker_index];
				Vector3 marker_pos(marker_data[0], marker_data[1], marker_data[2]);
				rigidbody.AddMarker(marker_id, marker_pos);
			}

			msg.AddRigidbody(rigidbody);
		}

		Instance()->Broadcast(msg);
	}
}

void OptitrackInput::ErrorHandler(int msg_type, char *msg)
{
	Log::Warning(msg);
}
