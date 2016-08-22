using UnityEngine;
using System.Xml;
using System;
using System.Collections.Generic;

/// <summary>
/// Track object through SlipStream (NatNet's local server that streams via XML to Unity)
/// </summary>
public class SlipStreamTrackedObject : MonoBehaviour
{
    // Name in OptiTrack
    public string TrackedName = "";

    public bool TrackPosition = true;
    public bool TrackOrientation = true;

    public bool DrawMarkers;

	void Start ()
    {
        SlipStream.Instance.PacketNotification += new PacketReceivedHandler(OnPacketReceived);
	}

    void OnPacketReceived(object sender, string Packet)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(Packet);

        XmlNodeList rbList = xmlDoc.GetElementsByTagName("RigidBody");

        for (int index = 0; index < rbList.Count; index++)
        {
            var rbName = rbList[index].Attributes["Name"].InnerText;

            if (String.Compare(rbName, TrackedName, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                if (TrackPosition)
                {
                    float x = (float)System.Convert.ToDouble(rbList[index].Attributes["x"].InnerText);
                    float y = (float)System.Convert.ToDouble(rbList[index].Attributes["y"].InnerText);
                    float z = (float)System.Convert.ToDouble(rbList[index].Attributes["z"].InnerText);

                    //== coordinate system conversion (right to left handed) ==--
                    z = -z;
                    Vector3 position = new Vector3(x, y, z);

                    this.transform.position = position;
                }

                if (TrackOrientation)
                {
                    float qx = (float)System.Convert.ToDouble(rbList[index].Attributes["qx"].InnerText);
                    float qy = (float)System.Convert.ToDouble(rbList[index].Attributes["qy"].InnerText);
                    float qz = (float)System.Convert.ToDouble(rbList[index].Attributes["qz"].InnerText);
                    float qw = (float)System.Convert.ToDouble(rbList[index].Attributes["qw"].InnerText);

                    //== coordinate system conversion (right to left handed) ==--

                    qz = -qz;
                    qw = -qw;

                    Quaternion orientation = new Quaternion(qx, qy, qz, qw);

                    this.transform.rotation = orientation;
                }


                if (DrawMarkers)
                {
                    for (var j = 0; j < rbList[index].ChildNodes.Count; j++)
                    {
                        var marker = rbList[index].ChildNodes[j];

                        int rbID = System.Convert.ToInt32(rbList[index].Attributes["ID"].InnerText);
                        int mID = System.Convert.ToInt32(marker.Attributes["ID"].InnerText);
                        float mx = (float)System.Convert.ToDouble(marker.Attributes["x"].InnerText);
                        float my = (float)System.Convert.ToDouble(marker.Attributes["y"].InnerText);
                        float mz = (float)System.Convert.ToDouble(marker.Attributes["z"].InnerText);

                        mz = -mz;

                        Vector3 mPosition = new Vector3(mx, my, mz);
                        Quaternion mOrientation = Quaternion.identity;

                        string markerName = String.Format("Marker{0}_{1}", rbID, mID);

                        GameObject markerObj;

                        markerObj = GameObject.Find(markerName);

                        if (markerObj == null)
                        {
                            markerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Vector3 scale = new Vector3(0.01f, 0.01f, 0.01f);
                            markerObj.transform.parent = transform;
                            markerObj.transform.localScale = scale;
                            markerObj.name = markerName;
                        }

                        markerObj.transform.position = mPosition;
                        markerObj.transform.rotation = mOrientation;
                    }
                }
            }

    
        }
    }
}
