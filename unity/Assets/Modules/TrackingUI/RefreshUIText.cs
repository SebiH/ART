using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Modules.Tracking;
using System.Text;

namespace Assets.Modules.TrackingUI
{
    public class RefreshUIText : MonoBehaviour
    {
        public PerformCalibration CalibrationScript;
        private Text _textbox;

        void Start()
        {
            _textbox = GetComponent<Text>();
        }

        void Update()
        {
            if (CalibrationScript != null)
            {
                StringBuilder sb = new StringBuilder();

                if (CalibrationScript.HasSteadyArucoPose)
                {
                    sb.AppendLine("Aruco: <color=#008000ff>✔</color>");
                }
                else
                {
                    sb.AppendLine("Aruco: <color=#ff0000ff>X</color>");
                }

                if (CalibrationScript.HasSteadyOpenVrPose)
                {
                    sb.AppendLine("OpenVR: <color=#008000ff>✔</color>");
                }
                else
                {
                    sb.AppendLine("OpenVR: <color=#ff0000ff>X</color>");
                }

                if (CalibrationScript.HasSteadyOptitrackCalibrationPose)
                {
                    sb.AppendLine("Optitrack Calibration: <color=#008000ff>✔</color>");
                }
                else
                {
                    sb.AppendLine("Optitrack Calibration: <color=#ff0000ff>X</color>");
                }

                if (CalibrationScript.HasSteadyOptitrackCameraPose)
                {
                    sb.AppendLine("Optitrack Camera: <color=#008000ff>✔</color>");
                }
                else
                {
                    sb.AppendLine("Optitrack Camera: <color=#ff0000ff>X</color>");
                }

                if (CalibrationScript.IsReadyForCalibration)
                {
                    sb.AppendLine("<b>Ready for calibration!</b>");
                }

                _textbox.text = sb.ToString();
            }
            else
            {
                Debug.LogWarning("No CalibrationScript set!");
            }
        }
    }
}
