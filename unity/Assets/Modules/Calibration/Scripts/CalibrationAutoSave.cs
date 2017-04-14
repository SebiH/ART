using Assets.Modules.Core;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class CalibrationAutoSave : MonoBehaviour
    {
        // In seconds
        public float SaveInterval = 5f;
        private Coroutine _saveRoutine;

        private void OnEnable()
        {
            _saveRoutine = StartCoroutine(AutoSave());
        }

        private void OnDisable()
        {
            if (_saveRoutine != null)
            {
                StopCoroutine(_saveRoutine);
            }
        }


        private IEnumerator AutoSave()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(Mathf.Max(1f, SaveInterval));

                if (CalibrationParams.HasStablePosition && CalibrationParams.HasStableRotation)
                {
                    var calib = new SerializableCalibration
                    {
                        PositionOffset = CalibrationParams.PositionOffset,
                        RotationOffset = CalibrationParams.RotationOffset
                    };

                    FileUtility.SaveToFile(Globals.CalibrationSavefile, JsonUtility.ToJson(calib));
                    Debug.Log("Autosaved calibration @ " + DateTime.Now.ToShortTimeString());
                }
                else
                {
                    Debug.Log("Calibration not stable enough for autosave");
                }
            }
        }
    }
}
