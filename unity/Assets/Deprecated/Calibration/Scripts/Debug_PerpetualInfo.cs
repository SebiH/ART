using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Assets.Modules.Calibration_Deprecated
{
    public class Debug_PerpetualInfo : MonoBehaviour
    {
        public Perpetual_Calibration Script;

        private Text _textBox;

        void OnEnable()
        {
            _textBox = GetComponent<Text>();
        }

        void Update()
        {
            _textBox.text = string.Format(@"Nearest Marker: {0}
HasWrongAngle? {1}
IsTooFarAway?? {2}
Cam-Table Angle: {3}
Cam-Marker Distance: {4}", Script.__nearestMarkerId, Script.__hasWrongAngle, Script.__isTooFarAway,
Script.__camTableAngle, Script.__camMarkerDistance);
        }
    }
}
