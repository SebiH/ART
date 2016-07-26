using Assets.Code.Graph;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class InteractiveSurfaceClient : MonoBehaviour
{
    public string ServerIp = "127.0.0.1";
    public int ServerPort = 8835;

    public GameObject Screen;
    public GameObject Cursor;
    public DataPoint Bar;

    private Vector2 DisplaySize;

    private TcpClient _client;
    private bool _isRunning;
    private Vector2 _currentPosition = Vector2.zero;
    private bool _hasNewPosition;

    void Start()
    {
        _client = new TcpClient();
        _client.Connect(ServerIp, ServerPort);
        _isRunning = true;

        Thread t = new Thread(new ThreadStart(ReceiveData));
        t.Start();

        DisplaySize = new Vector2(transform.localScale.x, transform.localScale.z);

        var oldScale = transform.localScale;
        transform.localScale = Vector3.one;
        Cursor.transform.localScale = Vector3.one * 0.1f;
        Screen.transform.localScale = oldScale;
    }

    private void ReceiveData()
    {
        var stream = _client.GetStream();

        var expectedFloats = 2;
        var byteBuffer = new byte[expectedFloats * sizeof(float)];
        var floatBuffer = new float[expectedFloats];

        while (_isRunning)
        {
            stream.Read(byteBuffer, 0, byteBuffer.Length);
            Buffer.BlockCopy(byteBuffer, 0, floatBuffer, 0, byteBuffer.Length);
            _currentPosition = new Vector2(floatBuffer[0], floatBuffer[1]);
            _hasNewPosition = true;
        }
    }

    void Update()
    {
        if (_hasNewPosition)
        {
            _hasNewPosition = false;
            Cursor.transform.localPosition = new Vector3((_currentPosition.x - 0.5f) * DisplaySize.x, 0, (_currentPosition.y - 0.5f) * DisplaySize.y);
            Bar.Height = 0f;
            Bar.TargetHeight = UnityEngine.Random.value * 2.5f;
        }
    }

    void OnDestroy()
    {
        _isRunning = false;
        _client.Close();
    }
}
