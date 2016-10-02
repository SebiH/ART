/*
 *	Adapted from NatNet's UnitySample.cpp
 */

#include <iostream>
#include <fstream>
#include <stdarg.h>
#include <stdio.h>
#include <tchar.h>
#include <conio.h>
#include <winsock2.h>
#include <string>
#include <sstream>
#include <string>
#include <map>
#include <memory>


#include "NatNet/NatNetTypes.h"
#include "NatNet/NatNetClient.h"
#include "NatNet/NatNetRepeater.h"   //== for transport of data over UDP to Unity3D
#include "tinyxml/tinyxml.h"  //== for xml encoding of Unity3D payload

//== Slip Stream globals ==--

cSlipStream *gSlipStream;
std::map<int, std::string> gBoneNames;

#pragma warning( disable : 4996 )

void __cdecl DataHandler(sFrameOfMocapData* data, void* pUserData);		// receives data from the server
void __cdecl MessageHandler(int msgType, char* msg);		            // receives NatNet error mesages
void resetClient();
int CreateClient(int iConnectionType);

unsigned int MyServersDataPort = 3130;
unsigned int MyServersCommandPort = 3131;

NatNetClient* theClient;
FILE* fp;

std::string szMyIPAddress;
std::string szServerIPAddress;
std::string szUnityIPAddress;
std::string szCaptureFilename;

int szLogLevel;

void SendXMLToUnity(sFrameOfMocapData *data, void* pUserData);

static inline void Log(const std::string &fmt_str, ...)
{
	if (szLogLevel > 0)
	{
		// Taken from http://stackoverflow.com/a/8098080/4090817
		int final_n, n = ((int)fmt_str.size()) * 2; /* Reserve two times as much as the length of the fmt_str */
		std::string str;
		std::unique_ptr<char[]> formatted;
		va_list ap;
		while (1)
		{
			formatted.reset(new char[n]); /* Wrap the plain char array into the unique_ptr */
			strcpy(&formatted[0], fmt_str.c_str());
			va_start(ap, fmt_str);
			final_n = vsnprintf(&formatted[0], n, fmt_str.c_str(), ap);
			va_end(ap);
			if (final_n < 0 || final_n >= n)
				n += abs(final_n - n + 1);
			else
				break;
		}

		printf(formatted.get());
	}
}

extern "C" __declspec(dllexport) void ReplayFromData(const char *filename)
{
	szUnityIPAddress = std::string("127.0.0.1");
	Log("Connecting to Unity3D on LocalMachine...\n");

	// read line for line, each line is a single package
	// after reaching end, we'll start from the beginning to get a continous stream
	std::string line;
	gSlipStream = new cSlipStream(szUnityIPAddress.c_str(), 16000);
	Log("Reading data from file %s ... \n", filename);

	long counter = 0;

	while (true)
	{
		auto file = std::ifstream(filename);

		while (std::getline(file, line))
		{
			Log("Sending new packet (%ld)\n", counter++);
			gSlipStream->Stream((unsigned char *)line.c_str(), line.length());
			_sleep(5);
		}

		Log("Reached end of file\n");
	}
}



extern "C" __declspec(dllexport) void StartServer(const char *OptitrackIp, const char *ListenIp, const char *UnityIp, const char *SaveFile, int LogLevel)
{
	int iResult;
	int iConnectionType = ConnectionType_Multicast;
	//int iConnectionType = ConnectionType_Unicast;

	szLogLevel = LogLevel;

	// parse command line args
	if (OptitrackIp != '\0')
	{
		szServerIPAddress = std::string(OptitrackIp);
		Log("Connecting to server at %s...\n", szServerIPAddress);
	}
	else
	{
		szServerIPAddress = std::string();
		Log("Connecting to server at LocalMachine\n");
	}

	if (ListenIp != '\0')
	{
		szMyIPAddress = std::string(ListenIp);
		Log("Connecting from %s...\n", szMyIPAddress);
	}
	else
	{
		szMyIPAddress = std::string();
		Log("Connecting from LocalMachine...\n");
	}

	if (UnityIp != '\0')
	{
		szUnityIPAddress = std::string(UnityIp);
		Log("Connecting to Unity3D at %s...\n", szUnityIPAddress);
	}
	else
	{
		szUnityIPAddress = std::string("127.0.0.1");
		Log("Connecting to Unity3D on LocalMachine...\n");
	}

	if (SaveFile != '\0')
	{
		szCaptureFilename = std::string(SaveFile);
		Log("Saving capture data to file %s...\n", szCaptureFilename);
	}

	gSlipStream = new cSlipStream(szUnityIPAddress.c_str(), 16000);

	// Create NatNet Client
	iResult = CreateClient(iConnectionType);
	if (iResult != ErrorCode_OK)
	{
		Log("Error initializing client.  See log for details.  Exiting");
		return;
	}
	else
	{
		Log("Client initialized and ready.\n");
	}


	// send/receive test request
	Log("[SampleClient] Sending Test Request\n");
	void* response;
	int nBytes;
	iResult = theClient->SendMessageAndWait("TestRequest", &response, &nBytes);
	if (iResult == ErrorCode_OK)
	{
		Log("[SampleClient] Received: %s", (char*)response);
	}

	// Retrieve Data Descriptions from server
	Log("\n\n[SampleClient] Requesting Data Descriptions...");
	sDataDescriptions* pDataDefs = NULL;
	int nBodies = theClient->GetDataDescriptions(&pDataDefs);
	if (!pDataDefs)
	{
		Log("[SampleClient] Unable to retrieve Data Descriptions.");
	}
	else
	{
		Log("[SampleClient] Received %d Data Descriptions:\n", pDataDefs->nDataDescriptions);
		for (int i = 0; i < pDataDefs->nDataDescriptions; i++)
		{
			Log("Data Description # %d (type=%d)\n", i, pDataDefs->arrDataDescriptions[i].type);
			if (pDataDefs->arrDataDescriptions[i].type == Descriptor_MarkerSet)
			{
				// MarkerSet
				sMarkerSetDescription* pMS = pDataDefs->arrDataDescriptions[i].Data.MarkerSetDescription;
				Log("MarkerSet Name : %s\n", pMS->szName);
				for (int i = 0; i < pMS->nMarkers; i++)
					Log("%s\n", pMS->szMarkerNames[i]);

			}
			else if (pDataDefs->arrDataDescriptions[i].type == Descriptor_RigidBody)
			{
				// RigidBody
				sRigidBodyDescription* pRB = pDataDefs->arrDataDescriptions[i].Data.RigidBodyDescription;
				Log("RigidBody Name : %s\n", pRB->szName);
				Log("RigidBody ID : %d\n", pRB->ID);
				Log("RigidBody Parent ID : %d\n", pRB->parentID);
				Log("Parent Offset : %3.2f,%3.2f,%3.2f\n", pRB->offsetx, pRB->offsety, pRB->offsetz);
				gBoneNames[pRB->ID] = pRB->szName;
			}
			else if (pDataDefs->arrDataDescriptions[i].type == Descriptor_Skeleton)
			{
				// Skeleton
				sSkeletonDescription* pSK = pDataDefs->arrDataDescriptions[i].Data.SkeletonDescription;
				Log("Skeleton Name : %s\n", pSK->szName);
				Log("Skeleton ID : %d\n", pSK->skeletonID);
				Log("RigidBody (Bone) Count : %d\n", pSK->nRigidBodies);
				for (int j = 0; j < pSK->nRigidBodies; j++)
				{
					sRigidBodyDescription* pRB = &pSK->RigidBodies[j];
					Log("  RigidBody Name : %s\n", pRB->szName);
					Log("  RigidBody ID : %d\n", pRB->ID);
					Log("  RigidBody Parent ID : %d\n", pRB->parentID);
					Log("  Parent Offset : %3.2f,%3.2f,%3.2f\n", pRB->offsetx, pRB->offsety, pRB->offsetz);

					// populate bone name dictionary for use in xml ==--
					gBoneNames[pRB->ID] = pRB->szName;
				}
			}
			else
			{
				Log("Unknown data type.");
				// Unknown
			}
		}
	}

	// Ready to receive marker stream!
	Log("\nClient is connected to server and listening for data...\n");
	int c;
	bool bExit = false;
	while (c = _getch())
	{
		switch (c)
		{
		case 'q':
			bExit = true;
			break;
		case 'r':
			resetClient();
			break;
		case 'p':
			sServerDescription ServerDescription;
			memset(&ServerDescription, 0, sizeof(ServerDescription));
			theClient->GetServerDescription(&ServerDescription);
			if (!ServerDescription.HostPresent)
			{
				Log("Unable to connect to server. Host not present. Exiting.");
				return;
			}
			break;
		case 'f':
		{
			sFrameOfMocapData* pData = theClient->GetLastFrameOfData();
			Log("Most Recent Frame: %d", pData->iFrame);
		}
		break;
		case 'm':	                        // change to multicast
			iResult = CreateClient(ConnectionType_Multicast);
			if (iResult == ErrorCode_OK)
				Log("Client connection type changed to Multicast.\n\n");
			else
				Log("Error changing client connection type to Multicast.\n\n");
			break;
		case 'u':	                        // change to unicast
			iResult = CreateClient(ConnectionType_Unicast);
			if (iResult == ErrorCode_OK)
				Log("Client connection type changed to Unicast.\n\n");
			else
				Log("Error changing client connection type to Unicast.\n\n");
			break;


		default:
			break;
		}
		if (bExit)
			break;
	}

	// Done - clean up.
	theClient->Uninitialize();
}

// Establish a NatNet Client connection
int CreateClient(int iConnectionType)
{
	// release previous server
	if (theClient)
	{
		theClient->Uninitialize();
		delete theClient;
	}

	// create NatNet client
	theClient = new NatNetClient(iConnectionType);

	// [optional] use old multicast group
	//theClient->SetMulticastAddress("224.0.0.1");

	// print version info
	unsigned char ver[4];
	theClient->NatNetVersion(ver);
	Log("NatNet Sample Client (NatNet ver. %d.%d.%d.%d)\n", ver[0], ver[1], ver[2], ver[3]);

	// Set callback handlers
	theClient->SetMessageCallback(MessageHandler);
	theClient->SetVerbosityLevel(Verbosity_Debug);
	theClient->SetDataCallback(DataHandler, theClient);	// this function will receive data from the server

	// Init Client and connect to NatNet server
	// to use NatNet default port assigments
	auto myIp = std::make_unique<char[]>(szMyIPAddress.length());
	strncpy(myIp.get(), szMyIPAddress.c_str(), szMyIPAddress.length());

	auto serverIp = std::make_unique<char[]>(szServerIPAddress.length());
	strncpy(serverIp.get(), szServerIPAddress.c_str(), szServerIPAddress.length());

	int retCode = theClient->Initialize(myIp.get(), serverIp.get());
	// to use a different port for commands and/or data:
	//int retCode = theClient->Initialize(szMyIPAddress, szServerIPAddress, MyServersCommandPort, MyServersDataPort);
	if (retCode != ErrorCode_OK)
	{
		Log("Unable to connect to server.  Error code: %d. Exiting", retCode);
		return ErrorCode_Internal;
	}
	else
	{
		// print server info
		sServerDescription ServerDescription;
		memset(&ServerDescription, 0, sizeof(ServerDescription));
		theClient->GetServerDescription(&ServerDescription);
		if (!ServerDescription.HostPresent)
		{
			Log("Unable to connect to server. Host not present. Exiting.");
			return 1;
		}
		Log("[SampleClient] Server application info:\n");
		Log("Application: %s (ver. %d.%d.%d.%d)\n", ServerDescription.szHostApp, ServerDescription.HostAppVersion[0],
			ServerDescription.HostAppVersion[1], ServerDescription.HostAppVersion[2], ServerDescription.HostAppVersion[3]);
		Log("NatNet Version: %d.%d.%d.%d\n", ServerDescription.NatNetVersion[0], ServerDescription.NatNetVersion[1],
			ServerDescription.NatNetVersion[2], ServerDescription.NatNetVersion[3]);
		Log("Client IP:%s\n", szMyIPAddress);
		Log("Server IP:%s\n", szServerIPAddress);
		Log("Server Name:%s\n\n", ServerDescription.szHostComputerName);
	}

	return ErrorCode_OK;

}

// Create XML from frame data and output to Unity
void SendFrameToUnity(sFrameOfMocapData *data, void *pUserData)
{
	if (data->Skeletons > 0)
	{
		// form XML document

		TiXmlDocument doc;
		TiXmlDeclaration* decl = new TiXmlDeclaration("1.0", "", "");
		doc.LinkEndChild(decl);

		TiXmlElement * root = new TiXmlElement("Stream");
		doc.LinkEndChild(root);

		TiXmlElement * skeletons = new TiXmlElement("Skeletons");
		root->LinkEndChild(skeletons);

		// skeletons first

		for (int i = 0; i < data->nSkeletons; i++)
		{
			TiXmlElement * skeleton = new TiXmlElement("Skeleton");
			skeletons->LinkEndChild(skeleton);

			TiXmlElement * bone;

			sSkeletonData skData = data->Skeletons[i]; // first skeleton ==--

			skeleton->SetAttribute("ID", skData.skeletonID);

			for (int i = 0; i < skData.nRigidBodies; i++)
			{
				sRigidBodyData rbData = skData.RigidBodyData[i];

				bone = new TiXmlElement("Bone");
				skeleton->LinkEndChild(bone);

				bone->SetAttribute("ID", rbData.ID);
				bone->SetAttribute("Name", gBoneNames[LOWORD(rbData.ID)].c_str());
				bone->SetDoubleAttribute("x", rbData.x);
				bone->SetDoubleAttribute("y", rbData.y);
				bone->SetDoubleAttribute("z", rbData.z);
				bone->SetDoubleAttribute("qx", rbData.qx);
				bone->SetDoubleAttribute("qy", rbData.qy);
				bone->SetDoubleAttribute("qz", rbData.qz);
				bone->SetDoubleAttribute("qw", rbData.qw);

				for (auto j = 0; j < rbData.nMarkers; j++)
				{
					auto marker = new TiXmlElement("Marker");
					bone->LinkEndChild(marker);

					marker->SetAttribute("ID", rbData.MarkerIDs[j]);
					marker->SetDoubleAttribute("x", rbData.Markers[j][0]);
					marker->SetDoubleAttribute("y", rbData.Markers[j][1]);
					marker->SetDoubleAttribute("z", rbData.Markers[j][2]);
				}
			}
		}

		// rigid bodies ==--

		TiXmlElement * rigidBodies = new TiXmlElement("RigidBodies");
		root->LinkEndChild(rigidBodies);

		for (int i = 0; i < data->nRigidBodies; i++)
		{
			sRigidBodyData rbData = data->RigidBodies[i];

			TiXmlElement * rb = new TiXmlElement("RigidBody");
			rigidBodies->LinkEndChild(rb);

			rb->SetAttribute("ID", rbData.ID);
			rb->SetAttribute("Name", gBoneNames[LOWORD(rbData.ID)].c_str());
			rb->SetDoubleAttribute("x", rbData.x);
			rb->SetDoubleAttribute("y", rbData.y);
			rb->SetDoubleAttribute("z", rbData.z);
			rb->SetDoubleAttribute("qx", rbData.qx);
			rb->SetDoubleAttribute("qy", rbData.qy);
			rb->SetDoubleAttribute("qz", rbData.qz);
			rb->SetDoubleAttribute("qw", rbData.qw);

			for (auto j = 0; j < rbData.nMarkers; j++)
			{
				auto marker = new TiXmlElement("Marker");
				rb->LinkEndChild(marker);

				marker->SetAttribute("ID", rbData.MarkerIDs[j]);
				marker->SetDoubleAttribute("x", rbData.Markers[j][0]);
				marker->SetDoubleAttribute("y", rbData.Markers[j][1]);
				marker->SetDoubleAttribute("z", rbData.Markers[j][2]);
			}
		}

		// convert xml document into a buffer filled with data ==--

		std::ostringstream stream;
		stream << doc;
		std::string str = stream.str();
		const char* buffer = str.c_str();

		// stream xml data over UDP via SlipStream ==--

		gSlipStream->Stream((unsigned char *)buffer, (int)strlen(buffer));

		if (szCaptureFilename.length() > 0)
		{
			// save data for later replay
			std::ofstream file;
			file.open(szCaptureFilename, std::ofstream::out | std::ofstream::app);
			file << buffer << std::endl;
			file.close();
		}
	}
}

// DataHandler receives data from the server
void __cdecl DataHandler(sFrameOfMocapData* data, void* pUserData)
{
	NatNetClient* pClient = (NatNetClient*)pUserData;

	Log("Received frame %d\n", data->iFrame);

	SendFrameToUnity(data, pUserData);
}

// MessageHandler receives NatNet error/debug messages
void __cdecl MessageHandler(int msgType, char* msg)
{
	Log("\n%s\n", msg);
}

void resetClient()
{
	int iSuccess;

	Log("\n\nre-setting Client\n\n.");

	iSuccess = theClient->Uninitialize();
	if (iSuccess != 0)
	{
		Log("error un-initting Client\n");
	}

	// client->initialize doesn't use const char * ...
	auto myIp = std::make_unique<char[]>(szMyIPAddress.length());
	strncpy(myIp.get(), szMyIPAddress.c_str(), szMyIPAddress.length());

	auto serverIp = std::make_unique<char[]>(szServerIPAddress.length());
	strncpy(serverIp.get(), szServerIPAddress.c_str(), szServerIPAddress.length());

	iSuccess = theClient->Initialize(myIp.get(), serverIp.get());
	if (iSuccess != 0)
	{
		Log("error re-initting Client\n");
	}
}
