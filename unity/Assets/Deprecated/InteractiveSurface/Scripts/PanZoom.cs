using UnityEngine;
using System;
using Assets.Modules.Surfaces;

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

        public bool TranslateX = true;
        public bool TranslateZ = true;
        public bool Zoom = true;
        public bool ZoomX = true;
        public string DisplayName = "Surface";
        public float HeightOffset = -0.05f;
        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 RotationOffset = Vector3.zero;

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
            if (msg.command == "panzoom" && SurfaceManager.Has(DisplayName))
            {
                var surface = SurfaceManager.Get(DisplayName);

                var payload = JsonUtility.FromJson<MessagePayload>(msg.payload);
                var x = TranslateX ? surface.PixelToUnityCoord(payload.posX) * payload.zoom : 0;
                var z = TranslateZ ? -surface.PixelToUnityCoord(payload.posY) * payload.zoom : 0;

                transform.position = surface.GetCornerPosition(Corner.TopLeft) + surface.Rotation * new Vector3(-x, HeightOffset, -z) + PositionOffset;
                if (Zoom)
                {
                    transform.localScale = new Vector3(ZoomX ? Mathf.Max(payload.zoom, 0.002f) : 1f, 0.4f + payload.zoom / 4f, Mathf.Max(payload.zoom, 0.002f));
                }
                transform.rotation = Quaternion.Euler(surface.Rotation.eulerAngles + RotationOffset);
            }
        }
    }
}
