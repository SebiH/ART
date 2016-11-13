using Assets.Modules.Tracking;
using System;
using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public class RemoteDisplayManager : MonoBehaviour
    {
        private float pixelCmRatio = 1;

        void OnEnable()
        {
            InteractiveSurfaceClient.Instance.OnMessageReceived += HandleCommand;
        }

        void OnDisable()
        {
            InteractiveSurfaceClient.Instance.OnMessageReceived -= HandleCommand;
        }

        private void HandleCommand(IncomingCommand cmd)
        {
            switch (cmd.command)
            {
                case "pixelCmRatio":
                    pixelCmRatio = float.Parse(cmd.payload);
                    Debug.Log("Setting PixelCmRatio to " + pixelCmRatio);
                    break;

                case "window-size":
                    // TODO: use cmd.origin
                    if (FixedDisplays.Has("Surface"))
                    {
                        var display = FixedDisplays.Get("Surface");
                        var windowSize = JsonUtility.FromJson<WindowSize>(cmd.payload);
                        display.DisplayResolution = new Resolution
                        {
                            width = windowSize.width,
                            height = windowSize.height
                        };

                        Debug.Log("Using remote display resolution " + windowSize.width + "x" + windowSize.height);
                    }
                    break;
            }
        }


        #region

        [Serializable]
        private class WindowSize
        {
            public int width;
            public int height;
        }

        #endregion
    }
}
