using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public class InteractiveSurfaceClient : MonoBehaviour
    {
        public static InteractiveSurfaceClient Instance { get; private set; }

        public string ServerIp = "127.0.0.1";
        public int ServerPort = 8835;

        public delegate void MessageHandler(IncomingCommand cmd);
        public event MessageHandler OnMessageReceived;

        // see: https://forum.unity3d.com/threads/c-tcp-ip-socket-how-to-receive-from-server.227259/
        private Socket _socket;
        private byte[] _receiveBuffer = new byte[256 * 256];

        void OnEnable()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Connect(ServerIp, ServerPort);
                _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), null);
            }
            catch (SocketException ex)
            {
                Debug.Log(ex.Message);
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

        private void ReceiveData(IAsyncResult asyncResult)
        {
            // Check how much bytes are received and call Endreceive to finalize handshake
            int received = _socket.EndReceive(asyncResult);

            if (received <= 0)
                return;

            // Copy the received data into new buffer , to avoid null bytes
            byte[] receivedData = new byte[received];
            Buffer.BlockCopy(_receiveBuffer, 0, receivedData, 0, received);

            // Process data
            UTF8Encoding encoding = new UTF8Encoding();
            var receivedText = encoding.GetString(receivedData);

            // Split up json classes, in case multiple classes got sent in one batch
            var receivedJsonMsgs = SplitJson(receivedText);

            if (OnMessageReceived != null)
            {
                foreach (var msg in receivedJsonMsgs)
                {
                    OnMessageReceived(JsonUtility.FromJson<IncomingCommand>(msg));
                }
            }

            // Start receiving again
            _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), null);
        }

        private void SendData(byte[] data)
        {
            SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
            socketAsyncData.SetBuffer(data, 0, data.Length);
            _socket.SendAsync(socketAsyncData);
        }

        public void SendCommand(WebCommand cmd)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            var rawData = encoding.GetBytes(JsonUtility.ToJson(cmd));
            SendData(rawData);
            Debug.Log(JsonUtility.ToJson(cmd));
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
    }
}
