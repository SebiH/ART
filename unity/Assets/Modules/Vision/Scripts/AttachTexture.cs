using Assets.Modules.Vision.CameraSources;
using Assets.Modules.Vision.Outputs;
using UnityEngine;

namespace Assets.Modules.Vision
{
    public class AttachTexture : MonoBehaviour
    {
        public Pipeline ProcessingPipeline;
        public DirectXOutput.Eye Eye = DirectXOutput.Eye.Left;

        public bool AutoAlign = true;

        private DirectXOutput _output;


        void Start()
        {
            VisionManager.Instance.CameraSourceChanged += Init;
            Init(VisionManager.Instance.ActiveCamera);
        }

        void Init(CameraSource cam)
        {
            // TODO: might depend on module..
            var imageWidth = cam.SourceWidth;
            var imageHeight = cam.SourceHeight;

            TextureFormat txFormat = (cam.SourceChannels == 4) ? TextureFormat.BGRA32 : TextureFormat.RGB24;

            var camTexture = new Texture2D(imageWidth, imageHeight, txFormat, false);
            camTexture.wrapMode = TextureWrapMode.Clamp;

            GetComponent<Renderer>().materials[0].SetTexture("_MainTex", camTexture);

            var texturePtr = camTexture.GetNativeTexturePtr();
            _output = new DirectXOutput(texturePtr, Eye);
            ProcessingPipeline.AddOutput(_output);

            if (AutoAlign)
            {
                var aspectRatio = new Vector2((float)(imageWidth) / (float)(imageHeight), -1);
                transform.localScale = new Vector3(aspectRatio.x, aspectRatio.y, 1.0f);

                var ovrCam = cam as OvrvisionCameraSource;

                if (ovrCam != null)
                {
                    transform.localPosition = new Vector3(0, 0, ovrCam.GetFocalPoint() + 0.02f);
                }
            }

        }

        void OnDestroy()
        {
            VisionManager.Instance.CameraSourceChanged -= Init;
            ProcessingPipeline.RemoveOutput(_output);
        }
    }
}
