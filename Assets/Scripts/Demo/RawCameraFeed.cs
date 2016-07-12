using UnityEngine;

namespace Assets.Scripts.Demo
{
    class RawCameraFeed : MonoBehaviour
    {
        public enum OutputType { Left, Right };
        public OutputType Output = OutputType.Left;

        // automatically aligns and scales object 
        public bool AutoAlign = true;

        void Start()
        {
            var ovrWrapper = OvrHandler.Instance;
            var imageWidth = ovrWrapper.OvrPro.imageSizeW;
            var imageHeight = ovrWrapper.OvrPro.imageSizeH;

            var camTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.BGRA32, false);
            camTexture.wrapMode = TextureWrapMode.Clamp;

            GetComponent<Renderer>().materials[0].SetTexture("_MainTex", camTexture);

            if (Output == OutputType.Left)
            {
                ovrWrapper.LeftTexturePtr = camTexture.GetNativeTexturePtr();
            }
            else
            {
                ovrWrapper.RightTexturePtr = camTexture.GetNativeTexturePtr();
            }


            if (AutoAlign)
            {
                var aspectRatio = new Vector2((float)(imageWidth) / (float)(imageHeight), -1);
                transform.localScale = new Vector3(aspectRatio.x, aspectRatio.y, 1.0f);

                float xOffset = 0;
                if (Output == OutputType.Right)
                {
                    xOffset = -0.032f;
                }
                else if (Output == OutputType.Left)
                {
                    xOffset = ovrWrapper.OvrPro.HMDCameraRightGap().x - 0.040f;
                }

                transform.localPosition = new Vector3(xOffset, 0.0f, ovrWrapper.OvrPro.GetFloatPoint() + 0.02f);
            }

        }

    }
}
