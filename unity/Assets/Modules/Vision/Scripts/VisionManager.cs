using Assets.Modules.Vision.CameraSources;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Vision
{
    public class VisionManager : MonoBehaviour
    {
        public static VisionManager Instance;

        public delegate void CameraSourceChangedHandler(CameraSource newCamera);
        public event CameraSourceChangedHandler CameraSourceChanged;

        private bool _isRunning = false;


        private CameraSource _prevCamSource;
        public CameraSource CamSource;


        private static void OnDebugMessage(string message)
        {
            Debug.Log("[ImageProcessing]: " + message);
        }

        void Awake()
        {
            Instance = this;

            ImageProcessing.RegisterLoggerCallback(OnDebugMessage);

            CamSource.Start();
            ImageProcessing.StartImageProcessing();
            _isRunning = true;
        }

        void Update()
        {
            if (_prevCamSource != CamSource)
            {
                Debug.Log("New camerasource detected, firing events");

                if (_prevCamSource != null)
                {
                    _prevCamSource.Stop();
                }

                CamSource.Start();
                _prevCamSource = CamSource;

                if (CameraSourceChanged != null)
                {
                    CameraSourceChanged(CamSource);
                }
            }
        }

        void OnDestroy()
        {
            _isRunning = false;
            ImageProcessing.StopImageProcessing();
            CamSource.Stop();

            // TODO: workaround since camera shouldn't be stopped
            ImageProcessing.SetEmptyCamera();
        }

        IEnumerator Start()
        {
            yield return StartCoroutine("CallPluginAtStartOfFrames");
        }


        private IEnumerator CallPluginAtStartOfFrames()
        {
            while (true)
            {
                if (_isRunning)
                {
                    GL.IssuePluginEvent(ImageProcessing.GetRenderEventFunc(), 1);
                }
                yield return new WaitForEndOfFrame();
            }

        }
    }
}
