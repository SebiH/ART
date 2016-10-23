using UnityEngine;

namespace Assets.Modules.Menu
{
    public class MenuInput : MonoBehaviour
    {
        private Menu _menu;

        public delegate void OnInputHandler(InputType input);
        public event OnInputHandler OnButtonPress;

        void OnEnable()
        {
            _menu = GetComponent<Menu>();
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
            if (OnButtonPress != null)
            {
                OnButtonPress(input);
            }
        }
    }
}
