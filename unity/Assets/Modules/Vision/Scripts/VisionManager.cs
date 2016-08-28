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
        //private CameraSource _camSource;
        //public CameraSource CamSource
        //{
        //    get { return _camSource; }
        //    set
        //    {
        //        var prevCamera = _camSource;
        //        _camSource = value;

        //        if (_isRunning && CameraSourceChanged != null)
        //        {
        //            prevCamera.Stop();
        //            _camSource.Start();
        //            CameraSourceChanged(_camSource);
        //        }
        //    }
        //}


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

                if (CamSource != null)
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
                    // Issue a plugin event with arbitrary integer identifier.
                    // The plugin can distinguish between different
                    // things it needs to do based on this ID.
                    // For our simple plugin, it does not matter which ID we pass here.
                    GL.IssuePluginEvent(ImageProcessing.GetRenderEventFunc(), 1);
                }

                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();
            }

        }
    }
}
