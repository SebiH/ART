using Assets.Modules.Calibration;
using Assets.Modules.Core;
using Assets.Modules.Surfaces;
using Assets.Modules.Tracking;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface.Scripts
{
    public class AdminCommandListener : MonoBehaviour
    {
        public OptitrackCalibrateDisplay DisplayCalibration;

        private void OnEnable()
        {
            RemoteSurfaceConnection.OnCommandReceived += OnCommand;
            StartCoroutine(SendUpdatesPeriodically());
        }

        private void OnDisable()
        {
            RemoteSurfaceConnection.OnCommandReceived -= OnCommand;
        }

        private IEnumerator SendUpdatesPeriodically()
        {
            if (DisplayCalibration)
            {
                while (isActiveAndEnabled)
                {
                    yield return new WaitForSeconds(DisplayCalibration.IsCalibrating ? 0.1f : 1f);
                    SendUpdates();
                }
            }
        }

        private void SendUpdates()
        {
            var status = new CalibrationStatus
            {
                topLeftCalibrated = DisplayCalibration.IsCornerCalibrated[(int)Corner.TopLeft],
                bottomLeftCalibrated = DisplayCalibration.IsCornerCalibrated[(int)Corner.BottomLeft],
                topRightCalibrated = DisplayCalibration.IsCornerCalibrated[(int)Corner.TopRight],
                bottomRightCalibrated = DisplayCalibration.IsCornerCalibrated[(int)Corner.BottomRight],
                calibrationStatus = DisplayCalibration.CalibrationProgress,
                isCalibrating = DisplayCalibration.IsCalibrating
            };
            RemoteSurfaceConnection.SendCommand(Globals.DefaultSurfaceName, "admin-cmd-calibration-status", JsonUtility.ToJson(status));
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
                        SendUpdates();
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

        [Serializable]
        private struct CalibrationStatus
        {
            public bool topLeftCalibrated;
            public bool topRightCalibrated;
            public bool bottomLeftCalibrated;
            public bool bottomRightCalibrated;
            public float calibrationStatus;
            public bool isCalibrating;
        }
    }
}
