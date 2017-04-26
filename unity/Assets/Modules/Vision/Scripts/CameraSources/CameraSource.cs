using System;
using UnityEngine;

namespace Assets.Modules.Vision.CameraSources
{
    public abstract class CameraSource : MonoBehaviour
    {
        protected bool _isRunning = false;

        protected virtual void OnEnable()
        {
            if (!_isRunning)
            {
                try
                {
                    _isRunning = true;
                    InitCamera();

                    // must be last to ensure camera is open
                    VisionManager.Instance.ActiveCamera = this;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public abstract void InitCamera();

        protected virtual void OnDisable()
        {
            if (VisionManager.Instance.ActiveCamera == this)
            {
                VisionManager.Instance.ActiveCamera = null;
            }
        }

        public int SourceWidth
        {
            get { return ImageProcessing.GetCamWidth(); }
        }

        public int SourceHeight
        {
            get { return ImageProcessing.GetCamHeight(); }
        }

        public int SourceChannels
        {
            get { return ImageProcessing.GetCamChannels(); }
        }

    }
}
