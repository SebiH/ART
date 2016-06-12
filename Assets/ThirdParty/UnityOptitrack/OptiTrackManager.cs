/**
 * Adapted from johny3212
 * Written by Matt Oskamp
 */
using UnityEngine;
using OptitrackManagement;
using System;
using System.Net;

public class OptiTrackManager : MonoBehaviour
{
    private static OptiTrackManager instance;

    public string myName;
    public float scale = 20.0f;

    public string OptitrackIPAddress = "127.0.0.1";
    public int OptitrackPort = 1511;

    // set this to wherever you want the center to be in your scene
    public Vector3 origin = Vector3.zero;

    private OptitrackSocket _socket = null;

    public SocketType ConnectionType = SocketType.Unicast;

    public enum SocketType
    {
        Multicast, Unicast
    };


    public static OptiTrackManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        instance = this;
    }

    ~OptiTrackManager()
    {
        Debug.Log("OptitrackManager: Destruct");
        _socket.Close();
    }

    void Start()
    {
        Debug.Log(myName + ": Initializing");

        switch (ConnectionType)
        {
            case SocketType.Multicast:
                _socket = new DirectMulticastSocketClient();
                break;

            case SocketType.Unicast:
                _socket = new DirectUnicastSocketClient();
                break;

            default:
                throw new Exception("Unknown ConnectionType " + ConnectionType.ToString());
        }

        _socket.Start(IPAddress.Parse(OptitrackIPAddress), OptitrackPort);
        Application.runInBackground = true;
    }

    public OptiTrackRigidBody getOptiTrackRigidBody(int index)
    {
        // only do this if you want the raw data
        if (_socket.IsInit())
        {
            DataStream networkData = _socket.GetDataStream();
            return networkData.getRigidbody(index);
        }
        else
        {
            _socket.Start(IPAddress.Parse(OptitrackIPAddress), OptitrackPort);
            return getOptiTrackRigidBody(index);
        }
    }

    public Vector3 getPosition(int rigidbodyIndex)
    {
        if (_socket.IsInit())
        {
            DataStream networkData = _socket.GetDataStream();
            Vector3 pos = origin + networkData.getRigidbody(rigidbodyIndex).position * scale;
            pos.x = -pos.x; // not really sure if this is the best way to do it
                            //pos.y = pos.y; // these may change depending on your configuration and calibration
                            //pos.z = -pos.z;
            return pos;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Quaternion getOrientation(int rigidbodyIndex)
    {
        // should add a way to filter it
        if (_socket.IsInit())
        {
            DataStream networkData = _socket.GetDataStream();
            Quaternion rot = networkData.getRigidbody(rigidbodyIndex).orientation;

            // change the handedness from motive
            //rot = new Quaternion(rot.z, rot.y, rot.x, rot.w); // depending on calibration

            // Invert pitch and yaw
            Vector3 euler = rot.eulerAngles;
            rot.eulerAngles = new Vector3(euler.x, -euler.y, euler.z); // these may change depending on your calibration

            return rot;
        }
        else
        {
            return Quaternion.identity;
        }
    }

    public void DeInitialize()
    {
        _socket.Close();
    }

    // Update is called once per frame
    void Update()
    {

    }
}