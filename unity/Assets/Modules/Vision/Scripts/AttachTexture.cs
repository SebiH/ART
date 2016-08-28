using Assets.Modules.Vision.CameraSources;
using Assets.Modules.Vision.Outputs;
using UnityEngine;

namespace Assets.Modules.Vision
{
    class AttachTexture : MonoBehaviour
    {
        public Pipeline ProcessingPipeline;
        public DirectXOutput.Eye Eye;

        public bool AutoAlign = true;

        private DirectXOutput _output;


        void Start()
        {
            VisionManager.Instance.CameraSourceChanged += Init;
        }

        void Init(CameraSource cam)
        {
            // TODO: might depend on module..
            var imageWidth = cam.SourceWidth;
            var imageHeight = cam.SourceHeight;

            var camTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.BGRA32, false);
            camTexture.wrapMode = TextureWrapMode.Clamp;

            GetComponent<Renderer>().materials[0].SetTexture("_MainTex", camTexture);

            var texturePtr = camTexture.GetNativeTexturePtr();
            _output = new DirectXOutput(texturePtr, Eye);
            ProcessingPipeline.AddOutput(_output);

            if (AutoAlign)
            {
                var aspectRatio = new Vector2((float)(imageWidth) / (float)(imageHeight), -1);
                transform.localScale = new Vector3(aspectRatio.x, aspectRatio.y, 1.0f);
                transform.localPosition = new Vector3(0, 0, cam.SourceFocalPoint + 0.02f);
            }

        }

        void OnDestroy()
        {
            VisionManager.Instance.CameraSourceChanged -= Init;
            ProcessingPipeline.RemoveOutput(_output);
        }
    }
}
