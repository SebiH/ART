using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Menu
{
    public class MenuInput : MonoBehaviour
    {
        public float ThrottlePeriod = 0.1f;

        public delegate void OnInputHandler(InputType input);
        public event OnInputHandler OnButtonPress;

        private Dictionary<int,float> _lastEventTime = new Dictionary<int, float>();

        void OnEnable()
        {
        }

        void OnDisable()
        {
        }

        void Update()
        {
            /* 
             * Current implementation restricted to win10 xbox controller
             * See http://wiki.unity3d.com/index.php?title=Xbox360Controller
             */

            if (Input.GetKeyDown("joystick button 0"))
            {
                // A button
                RaiseInputEvent(InputType.Confirm);
            }

            if (Input.GetKeyDown("joystick button 1"))
            {
                // B Button
                RaiseInputEvent(InputType.Cancel);
            }

            if (Input.GetKeyDown("joystick button 2"))
            {
                // X Button
                //RaiseInputEvent(InputType.Confirm);
            }

            if (Input.GetKeyDown("joystick button 3"))
            {
                // Y Button
                //RaiseInputEvent(InputType.Confirm);
            }

            if (Input.GetKeyDown("joystick button 6"))
            {
                // Back button
                RaiseInputEvent(InputType.Back);
            }

            if (Input.GetKeyDown("joystick button 7"))
            {
                // Start Button
                RaiseInputEvent(InputType.Start);
            }


            var dpadX = Input.GetAxis("dpadx");
            if (dpadX < 0)
            {
                // left
                RaiseInputEvent(InputType.Left);
            }
            else if (dpadX > 0)
            {
                // right
                RaiseInputEvent(InputType.Right);
            }

            var dpadY = Input.GetAxis("dpady");
            if (dpadY < 0)
            {
                // down
                RaiseInputEvent(InputType.Down);
            }
            else if (dpadY > 0)
            {
                // up
                RaiseInputEvent(InputType.Up);
            }
        }

        private void RaiseInputEvent(InputType input)
        {
            var inputIndex = (int)input;

            // don't raise too many events of the same time too quickly
            if (_lastEventTime.ContainsKey(inputIndex) && _lastEventTime[inputIndex] + ThrottlePeriod > Time.time)
            {
                return;
            }

            if (OnButtonPress != null)
            {
                OnButtonPress(input);
                _lastEventTime[inputIndex] = Time.time;
            }
        }
    }
}
