using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class InteractiveSurfaceClient : MonoBehaviour
{
    public string ServerIp = "127.0.0.1";
    public int ServerPort = 8835;

    public GameObject Screen;
    public GameObject Cursor;

    private Vector2 DisplaySize;

    private TcpClient _client;
    private bool _isRunning;
    private Vector2 _currentPosition = Vector2.zero;

    void Start()
    {
        _client = new TcpClient();
        _client.Connect(ServerIp, ServerPort);
        _isRunning = true;

        Thread t = new Thread(new ThreadStart(ReceiveData));
        t.Start();

        DisplaySize = transform.localScale;

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
            Debug.Log(_currentPosition);
        }
    }

    void Update()
    {
        Cursor.transform.localPosition = new Vector3((_currentPosition.x - 0.5f) * DisplaySize.x, 0, (_currentPosition.y - 0.5f) * DisplaySize.y);
    }

    void OnDestroy()
    {
        _isRunning = false;
        _client.Close();
    }
}
