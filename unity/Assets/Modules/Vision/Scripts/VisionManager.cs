using Assets.Modules.Vision.CameraSources;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Vision
{
    public class VisionManager : MonoBehaviour
    {
        public static VisionManager Instance;

        private CameraSource _activeCamera;
        public CameraSource ActiveCamera
        {
            get { return _activeCamera; }
            set
            {
                _activeCamera = value;
                if (CameraSourceChanged != null)
                {
                    CameraSourceChanged(value);
                }
            }
        }

        public delegate void CameraSourceChangedHandler(CameraSource newCamera);
        public event CameraSourceChangedHandler CameraSourceChanged;

        private bool _isRunning = false;

        private static void OnDebugMessage(string message)
        {
            Debug.Log("[ImageProcessing]: " + message);
        }

        void OnEnable()
        {
            Instance = this;

            ImageProcessing.RegisterLoggerCallback(OnDebugMessage);

            ImageProcessing.StartImageProcessing();
            _isRunning = true;
        }


        void OnDisable()
        {
            _isRunning = false;
            ImageProcessing.StopImageProcessing();
        }

        System.IntPtr RenderEventFunc;

        IEnumerator Start()
        {
            RenderEventFunc = ImageProcessing.GetRenderEventFunc();
            yield return StartCoroutine("CallPluginAtStartOfFrames");
        }

        private IEnumerator CallPluginAtStartOfFrames()
        {
            while (true)
            {
                if (_isRunning)
                {
                    GL.IssuePluginEvent(RenderEventFunc, 1);
                }
                yield return new WaitForEndOfFrame();
            }

        }
    }
}
