using UnityEngine;
using System;
using Assets.Modules.Tracking;

namespace Assets.Modules.InteractiveSurface
{

    public class PanZoom : MonoBehaviour
    {
        [Serializable]
        private struct MessagePayload
        {
            public float posX;
            public float posY;
            public float zoom;
        }

        public string DisplayName = "Surface";
        public float HeightOffset = -0.05f;

        void OnEnable()
        {
            InteractiveSurfaceClient.Instance.OnMessageReceived += OnMessage;
        }

        void OnDisable()
        {
            InteractiveSurfaceClient.Instance.OnMessageReceived -= OnMessage;
        }


        private void OnMessage(IncomingCommand msg)
        {
            if (msg.command == "panzoom" && FixedDisplays.Has(DisplayName))
            {
                var display = FixedDisplays.Get(DisplayName);

                var payload = JsonUtility.FromJson<MessagePayload>(msg.payload);
                var x = DisplayUtility.PixelToUnityCoord(payload.posX) * payload.zoom;
                var z = -DisplayUtility.PixelToUnityCoord(payload.posY) * payload.zoom;

                transform.localScale = new Vector3(Mathf.Max(payload.zoom, 0.01f), 0.5f, Mathf.Max(payload.zoom, 0.01f));
                transform.position = display.GetCornerPosition(Corner.TopLeft) + display.Rotation * new Vector3(-x, HeightOffset, -z);
                transform.rotation = display.Rotation;
            }
        }
    }
}
