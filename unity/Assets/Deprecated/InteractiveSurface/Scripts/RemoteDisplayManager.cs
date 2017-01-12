using Assets.Modules.Surfaces;
using System;
using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public class RemoteDisplayManager : MonoBehaviour
    {
        public string DisplayName = "Surface";

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
            // TODO1
            switch (cmd.command)
            {
                case "pixelCmRatio":
                    if (SurfaceManager.Instance.Has(DisplayName))
                    {
                        var display = SurfaceManager.Instance.Get(DisplayName);
                        display.PixelToCmRatio = float.Parse(cmd.payload.Trim('"'));
                        Debug.Log("Setting PixelCmRatio to " + display.PixelToCmRatio);
                    }
                    break;

                case "window-size":
                    // TODO: use cmd.origin
                    if (SurfaceManager.Instance.Has(DisplayName))
                    {
                        var display = SurfaceManager.Instance.Get(DisplayName);
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
            public int width = 0;
            public int height = 0;
        }

        #endregion
    }
}
