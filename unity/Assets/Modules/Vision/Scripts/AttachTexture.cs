using Assets.Modules.Tracking;
using Assets.Modules.Vision.CameraSources;
using Assets.Modules.Vision.Outputs;
using UnityEngine;

namespace Assets.Modules.Vision
{
    public class AttachTexture : MonoBehaviour
    {
        public Pipeline ProcessingPipeline;
        public DirectXOutput.Eye Eye = DirectXOutput.Eye.Left;

        // https://gopro.com/help/articles/Question_Answer/HERO4-Field-of-View-FOV-Information
        public float CameraFoVHorizontal = 94.4f;
        public float CameraFoVVertical = 55f;
        public float FocalPoint = 0.0219f;

        public bool AutoAlign = true;

        private DirectXOutput _output;


        private void OnEnable()
        {
            VisionManager.Instance.CameraSourceChanged += Init;
            Init(VisionManager.Instance.ActiveCamera);
        }

        private void OnDisable()
        {
            VisionManager.Instance.CameraSourceChanged -= Init;
            ProcessingPipeline.RemoveOutput(_output);
        }


        private void Init(CameraSource cam)
        {
            if (cam == null)
            {
                return;
            }

            // TODO: might depend on module..
            var imageWidth = cam.SourceWidth;
            var imageHeight = cam.SourceHeight;

            TextureFormat txFormat = (cam.SourceChannels == 4) ? TextureFormat.BGRA32 : TextureFormat.RGB24;

            var camTexture = new Texture2D(imageWidth, imageHeight, txFormat, false, false);
            camTexture.wrapMode = TextureWrapMode.Clamp;
            //camTexture.filterMode = FilterMode.Point;

            GetComponent<Renderer>().materials[0].SetTexture("_MainTex", camTexture);
            GetComponent<MeshFilter>().mesh.MarkDynamic();

            var texturePtr = camTexture.GetNativeTexturePtr();
            _output = new DirectXOutput(texturePtr, Eye);
            ProcessingPipeline.AddOutput(_output);

            if (AutoAlign)
            {
                var width = FocalPoint * Mathf.Tan(Mathf.Deg2Rad * CameraFoVHorizontal / 2f) * 2f;
                var height = FocalPoint * Mathf.Tan(Mathf.Deg2Rad * CameraFoVVertical / 2f) * 2f;

                transform.localScale = new Vector3(width, -height, 1.0f);
                transform.localPosition = new Vector3(0, 0, FocalPoint);
            }

        }

        // taken from OvrVision Pro
        private float GetImageBaseHeight(OvrvisionCameraSource.Quality opentype)
        {
            float res = 960.0f;
            switch (opentype)
            {
                case  OvrvisionCameraSource.Quality.Q2560x1920x15:
                    res = 1920.0f;
                    break;
                case OvrvisionCameraSource.Quality.Q1920x1080x30:
                    res = 1920.0f;
                    break;
                case OvrvisionCameraSource.Quality.Q1280x960x45:
                    res = 960.0f;
                    break;
                case OvrvisionCameraSource.Quality.Q960x950x60:
                    res = 960.0f;
                    break;
                case OvrvisionCameraSource.Quality.Q1280x800x60:
                    res = 960.0f;
                    break;
                case OvrvisionCameraSource.Quality.Q640x480x90:
                    res = 480.0f;
                    break;
                case OvrvisionCameraSource.Quality.Q320x240x120:
                    res = 240.0f;
                    break;
                default:
                    break;
            }

            return res;
        }
    }
}
