using Assets.Modules.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public class RemoteSurfaceConnection : MonoBehaviour
    {
        #region Serializing

        [Serializable]
        private class InPacket
        {
            public string origin = "";
            public string command = "";
            public string payload = "";
        }

        [Serializable]
        private class OutPacket
        {
            public string target = "";
            public string command = "";
            public string payload = "";
        }

        #endregion

        // see: https://forum.unity3d.com/threads/c-tcp-ip-socket-how-to-receive-from-server.227259/
        private Socket _socket;
        private byte[] _receiveBuffer = new byte[256 * 256];
        private Queue _queuedCommands;

        void OnEnable()
        {
            _queuedCommands = Queue.Synchronized(new Queue());
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Connect(Globals.SurfaceServerIp, Globals.SurfaceServerPort);
                _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), null);
                Debug.Log("Connection to web server established");
            }
            catch (SocketException ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        void OnDisable()
        {
            try
            {
                _socket.Disconnect(false);
            }
            catch (SocketException ex)
            {
                Debug.Log(ex.Message);
            }
        }

        void FixedUpdate()
        {
            var surfaceManager = SurfaceManager.Instance;

            while (_queuedCommands.Count > 0)
            {
                var cmd = _queuedCommands.Dequeue() as InPacket;
                if (surfaceManager.Has(cmd.origin))
                {
                    var surface = surfaceManager.Get(cmd.origin);
                    surface.TriggerAction(cmd.command, cmd.payload);
                }
            }
        }


        private void ReceiveData(IAsyncResult asyncResult)
        {
            // Check how much bytes are received and call Endreceive to finalize handshake
            int received = _socket.EndReceive(asyncResult);

            if (received <= 0)
            {
                return;
            }

            // Copy the received data into new buffer , to avoid null bytes
            byte[] receivedData = new byte[received];
            Buffer.BlockCopy(_receiveBuffer, 0, receivedData, 0, received);

            // Process data
            UTF8Encoding encoding = new UTF8Encoding();
            var receivedText = encoding.GetString(receivedData);

            // Split up json classes, in case multiple classes got sent in one batch
            var receivedJsonMsgs = SplitJson(receivedText);

            foreach (var msg in receivedJsonMsgs)
            {
                var incomingCmd = JsonUtility.FromJson<InPacket>(msg);
                // messages have to be handled in main update() thread, to avoid possible threading issues in handlers
                _queuedCommands.Enqueue(incomingCmd);
            }


            // Start receiving again
            _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), null);
        }

        // TODO: breaks easily, but sufficient for current purpose
        private IEnumerable<string> SplitJson(string text)
        {
            var jsonPackets = new List<string>();

            int leftBracketIndex = -1;
            int rightBracketIndex = -1;

            int bracketCounter = 0;
            var chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                char ch = chars[i];

                if (ch == '{')
                {
                    if (bracketCounter == 0)
                    {
                        leftBracketIndex = i;
                    }

                    bracketCounter++;
                }
                else if (ch == '}')
                {
                    bracketCounter--;

                    if (bracketCounter <= 0)
                    {
                        rightBracketIndex = i;
                        bracketCounter = 0;

                        jsonPackets.Add(text.Substring(leftBracketIndex, rightBracketIndex - leftBracketIndex + 1));

                        leftBracketIndex = -1;
                        rightBracketIndex = -1;
                    }
                }

            }

            return jsonPackets;
        }

        private void SendData(byte[] data)
        {
            SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
            socketAsyncData.SetBuffer(data, 0, data.Length);
            _socket.SendAsync(socketAsyncData);
        }

        public void SendMessage(string target, string command, string payload)
        {
            var packet = new OutPacket
            {
                target = target,
                command = command,
                payload = payload
            };

            var encoding = new UTF8Encoding();
            var rawData = encoding.GetBytes(JsonUtility.ToJson(packet));
            SendData(rawData);
        }
    }
}
