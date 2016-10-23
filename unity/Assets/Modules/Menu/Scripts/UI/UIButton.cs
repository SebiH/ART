using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Modules.Menu
{
    public class UIButton : UIElement
    {
        public string ButtonText;
        public UnityEvent ButtonClicked;

        public override bool IsSelectable { get { return true; } }

        public override GameObject CreateElement()
        {
            var element = base.CreateElement();
            var textContainer = element.transform.GetChild(0).GetComponent<Text>();
            textContainer.text = ButtonText;
            return element;
        }

        public override void HandleInput(InputType input)
        {
            MenuToast.Instance.Info(input.ToString());
            if (input == InputType.Confirm && ButtonClicked != null)
            {
                ButtonClicked.Invoke();
            }
        }
    }
}
