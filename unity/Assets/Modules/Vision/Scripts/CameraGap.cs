using Assets.Modules.Vision.CameraSources;
using UnityEngine;

namespace Assets.Modules.Vision
{
    public class CameraGap : MonoBehaviour
    {
        public float Gap = 0f;
        public bool AutoAdjust = true;

        public Transform LeftEye;
        public Transform RightEye;

        private void Update()
        {
            if (AutoAdjust)
            {
                if (VisionManager.Instance.ActiveCamera is OvrvisionCameraSource)
                {
                    var ovrCam = VisionManager.Instance.ActiveCamera as OvrvisionCameraSource;
                    Gap = ovrCam.GetHMDRightGap().x / 2;

                    // original code - AttachTexture
                    //var IMAGE_ZOFFSET = 0.02f;
                    // note - hmdrightgap is roughly 0.058f
                    //float xOffset = (Eye == DirectXOutput.Eye.Left) ? -0.032f : ovrCam.GetHMDRightGap().x - 0.040f;
                    //transform.localPosition = new Vector3(xOffset, 0, ovrCam.GetFocalPoint() + IMAGE_ZOFFSET);

                    // original code - CameraPropertiesListener
                    //if (LeftEye) { LeftEye.localPosition = _originalLeftEyePosition + new Vector3(offset, 0, 0); }
                    //if (RightEye) { RightEye.localPosition = _originalRightEyePosition + new Vector3(-offset, 0, 0); }
                }
                else
                {
                    // auto adjust not possible -> do nothing
                }
            }

            // ??? (ovrvision uses 0.032 for left, 0.04 for right -- so theres a 0.008f offset between them???) ???
            // ??? and artoolkit markers are more accurate if left eye is adjusted by this offset) ???
            float strangeOffset = 0.008f;

            // apply gap
            LeftEye.localPosition = new Vector3(-Gap + strangeOffset, 0, LeftEye.localPosition.z);
            RightEye.localPosition = new Vector3(Gap, 0, RightEye.localPosition.z);
        }
    }
}
