using System;
using System.Net.Sockets;
using UnityEngine;

public class InteractiveSurfaceClient : MonoBehaviour
{
    public static InteractiveSurfaceClient Instance { get; private set; }

    public string ServerIp = "127.0.0.1";
    public int ServerPort = 8835;

    public delegate void MessageHandler(string jsonContent);
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
        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
        var receivedText = encoding.GetString(receivedData);
        Debug.Log(receivedText);

        // Start receiving again
        _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), null);
    }

    private void SendData(byte[] data)
    {
        SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
        socketAsyncData.SetBuffer(data, 0, data.Length);
        _socket.SendAsync(socketAsyncData);
    }

    public void SendCommand(string jsonData)
    {

    }
}
