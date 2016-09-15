using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Tracking.Editor
{
    [CustomEditor(typeof(PerformCalibration))]
    class CalibrationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Perform Calibration") && Application.isPlaying)
            {
                var script = target as PerformCalibration;
                script.Calibrate();
            }
        }
    }
}
