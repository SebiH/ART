/**
 * Adapted from johny3212
 * Written by Matt Oskamp
 */

using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;

namespace OptitrackManagement
{
    public class DirectMulticastSocketClient : OptitrackSocket
    {
        private Socket client;

        //private int _dataPort = 1511;
        //private int _commandPort = 1510;
        //private string _multicastIPAddress = "239.255.42.99";

        private void StartClient(IPAddress ip, int dataPort)
        {
            // Connect to a remote device.
            try
            {

                Debug.Log("[UDP] Starting client");
                _dataStream = new DataStream();
                client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                //client.ExclusiveAddressUse = false;

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, dataPort);
                client.Bind(ipep);

                client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));

                _isInitRecieveStatus = Receive(client);
                _isIsActiveThread = _isInitRecieveStatus;

            }
            catch (Exception e)
            {
                Debug.LogError("[UDP] DirectMulticastSocketClient: " + e.ToString());
            }
        }


        override public void Start(IPAddress ip, int port)
        {
            StartClient(ip, port);
        }

        override public void Close()
        {
            _isIsActiveThread = false;
        }
    }
}
