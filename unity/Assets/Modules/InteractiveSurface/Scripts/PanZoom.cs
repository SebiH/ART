using UnityEngine;
using System.Collections;
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
                var height = 1f;

                var payload = JsonUtility.FromJson<MessagePayload>(msg.payload);
                var x = DisplayUtility.PixelToUnityCoord(payload.posX) * payload.zoom;
                var y = height/2;
                var z = -DisplayUtility.PixelToUnityCoord(payload.posY) * payload.zoom;

                transform.position = display.GetCornerPosition(Corner.TopLeft) + new Vector3(x, y, z) * -1;
                transform.localScale = new Vector3(payload.zoom / 100f, height, payload.zoom / 100f);
            }
        }
    }
}
