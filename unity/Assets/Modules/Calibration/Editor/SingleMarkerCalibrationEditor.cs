using Assets.Modules.Tracking;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Modules.Calibration
{
    [CustomEditor(typeof(SingleMarker_PerformCalibration))]
    class SingleMarkerCalibrationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Perform Calibration") && Application.isPlaying)
            {
                var script = target as SingleMarker_PerformCalibration;
                script.Calibrate();
            }

            if (GUILayout.Button("Switch to Calibrated Scene") && Application.isPlaying)
            {
                SceneManager.LoadScene("Scenes/CalibratedTestScene");
            }
        }
    }
}
