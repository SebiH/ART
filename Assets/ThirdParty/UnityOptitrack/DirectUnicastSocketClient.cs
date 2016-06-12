using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace OptitrackManagement
{
    class DirectUnicastSocketClient : OptitrackSocket
    {
        private Socket _client;

        override public void Start(IPAddress ip, int port)
        {
            try
            {
                Debug.Log("[UDP] Starting unicast client");
                _dataStream = new DataStream();

                _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _client.Connect(ip, port);

                _isInitRecieveStatus = Receive(_client);
                _isIsActiveThread = _isInitRecieveStatus;
            }
            catch (Exception e)
            {
                Debug.LogError("[UDP] Optitrack Unicast Socket: " + e.ToString());
            }


        }

        override public void Close()
        {
            _isIsActiveThread = false;
        }

    }
}
