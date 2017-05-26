using Assets.Modules.Core;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public static class RemoteSurfaceConnection
    {
        const int BUFFER_SIZE = 10 * 1024 * 1024;

        private static Socket _socket;
        private static AsyncCallback _receiveCallback = new AsyncCallback(ReceiveData);
        private static byte[] _receiveBuffer = new byte[BUFFER_SIZE];
        private static int _receiveBufferOffset = 0;
        private static int _expectedPacketSize = -1;

        private static UTF8Encoding _encoding = new UTF8Encoding();
        private static LockFreeQueue<InPacket> _queuedCommands = new LockFreeQueue<InPacket>();

        public delegate void CommandReceivedHandler(string cmd, string payload);
        public static event CommandReceivedHandler OnCommandReceived;

        static RemoteSurfaceConnection()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            GameLoop.Instance.OnUpdate += Update;
            GameLoop.Instance.OnGameEnd += OnDisable;

            try
            {
                _socket.Connect(Globals.SurfaceServerIp, Globals.SurfaceServerPort);
                _socket.BeginReceive(_receiveBuffer, _receiveBufferOffset, _receiveBuffer.Length - _receiveBufferOffset, SocketFlags.None, _receiveCallback, null);
                Debug.Log("Connection to web server established");
            }
            catch (SocketException ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private static void OnDisable()
        {
            try
            {
                if (_socket.Connected)
                {
                    _socket.Close();
                }
            }
            catch (SocketException ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private static void Update()
        {
            var surfaceManager = SurfaceManager.Instance;

            InPacket packet = new InPacket();
            while (_queuedCommands.Dequeue(out packet))
            {
                if (surfaceManager && surfaceManager.Has(packet.origin))
                {
                    var surface = surfaceManager.Get(packet.origin);
                    surface.TriggerAction(packet.command, packet.payload);
                }

                if (OnCommandReceived != null)
                {
                    OnCommandReceived(packet.command, packet.payload);
                }
            }
        }

        /*
         *  Message format:
         *  \0\0\0 (Packet header as string) \0 (Actual packet json string)
         */

        private static bool HasPacketHeader(int offset)
        {
            if (offset + 2 >= _receiveBuffer.Length)
            {
                return false;
            }

            if (_receiveBuffer[offset] == '\0' &&
                _receiveBuffer[offset + 1] == '\0' &&
                _receiveBuffer[offset + 2] == '\0')
            {
                return true;
            }

            return false;
        }

        private static PacketHeader GetPacketHeader(int offset)
        {
            var start = offset + 3;
            var end = start;
            while (end < _receiveBuffer.Length && _receiveBuffer[end] != '\0')
            {
                // searching ...
                end++;
            }

            if (end >= _receiveBuffer.Length)
            {
                throw new OverflowException("Receive buffer overflow");
            }

            // don't want to deal with integer formatting, so it's transmitted as text instead
            byte[] packetSizeRaw = new byte[end - start + 1];
            Buffer.BlockCopy(_receiveBuffer, start, packetSizeRaw, 0, packetSizeRaw.Length);
            var packetSizeText = _encoding.GetString(packetSizeRaw);

            return new PacketHeader {
                PacketSize = int.Parse(packetSizeText),
                PacketStartOffset = end + 1
            };
        }

        private static void ReceiveData(IAsyncResult asyncResult)
        {
            int numReceived = _socket.EndReceive(asyncResult);
            Debug.Assert(numReceived >= 0, "Received negative amount of bytes from surface connection");

            var processingOffset = 0;
            var bufferEnd = _receiveBufferOffset + numReceived;

            while (processingOffset < bufferEnd)
            {
                if (_expectedPacketSize <= 0)
                {
                    if (HasPacketHeader(processingOffset))
                    {
                        var header = GetPacketHeader(processingOffset);
                        processingOffset = header.PacketStartOffset;
                        _expectedPacketSize = header.PacketSize;
                    }
                    else
                    {
                        Debug.LogWarning("Invalid packet received, skipping ahead!");
                        while (processingOffset < bufferEnd && !HasPacketHeader(processingOffset))
                        {
                            processingOffset++;
                        }

                    }
                }
                else if (processingOffset + _expectedPacketSize <= bufferEnd)
                {
                    byte[] rawPacket = new byte[_expectedPacketSize];
                    Buffer.BlockCopy(_receiveBuffer, processingOffset, rawPacket, 0, rawPacket.Length);
                    var packet = _encoding.GetString(rawPacket);

                    try
                    {
                        // messages have to be handled in main update() thread, to avoid possible threading issues in handlers
                        var incomingCmd = JsonUtility.FromJson<InPacket>(packet);
                        _queuedCommands.Enqueue(incomingCmd);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }

                    processingOffset += _expectedPacketSize;
                    _expectedPacketSize = -1;
                }
                else
                {
                    // neither header nor complete package
                    // -> currently incomplete packet in buffer, wait for rest
                    break;
                }
            }

            if (processingOffset == bufferEnd)
            {
                // cleared buffer entirely, no need to rearrange memory due to incomplete packet
                _receiveBufferOffset = 0;
            }
            else
            {
                // incomplete packet in buffer, move to front
                _receiveBufferOffset = bufferEnd - processingOffset;
                Buffer.BlockCopy(_receiveBuffer, processingOffset, _receiveBuffer, 0, _receiveBufferOffset);
            }

            
            if (_receiveBuffer.Length - _receiveBufferOffset < 100)
            {
                var error = "Receive buffer getting too small, aborting receive";
                Debug.LogError(error);
                throw new OverflowException(error);
            }

            _socket.BeginReceive(_receiveBuffer, _receiveBufferOffset, _receiveBuffer.Length - _receiveBufferOffset, SocketFlags.None, _receiveCallback, null);
        }

        private static void SendData(byte[] data)
        {
            SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
            socketAsyncData.SetBuffer(data, 0, data.Length);
            _socket.SendAsync(socketAsyncData);
        }

        public static void SendCommand(string target, string command, string payload)
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

        #region Helper classes

        private struct PacketHeader
        {
            public int PacketSize;
            public int PacketStartOffset;
        }

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

    }
}
