using Assets.Modules.Calibration;
using Assets.Modules.Surfaces;
using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface.Scripts
{
    public class AdminCommandListener : MonoBehaviour
    {
        public OptitrackCalibrateDisplay DisplayCalibration;

        private void OnEnable()
        {
            RemoteSurfaceConnection.OnCommandReceived += OnCommand;
        }

        private void OnDisable()
        {
            RemoteSurfaceConnection.OnCommandReceived -= OnCommand;
        }

        private void OnCommand(string cmd, string payload)
        {
            switch (cmd)
            {
                case "admin-cmd-set-corner":
                    if (DisplayCalibration)
                    {
                        var corner = (Corner)(int.Parse(payload.Replace("\"", "")));
                        DisplayCalibration.CurrentCorner = corner;
                        DisplayCalibration.StartCalibration();
                    }
                    break;
                case "admin-cmd-set-surface":
                    if (DisplayCalibration)
                    {
                        DisplayCalibration.CommitFixedDisplay();
                    }
                    break;
                case "admin-cmd-reset-calibration":
                    CalibrationParams.Reset();
                    break;
                case "admin-cmd-save-surfaces":
                    SurfaceFileLoader.SaveToFile("default_displays.json");
                    break;
            }
        }
    }
}
