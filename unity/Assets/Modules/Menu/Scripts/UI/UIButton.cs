using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Menu
{
    public class UIButton : UIElement
    {
        public string ButtonText;

        public override bool IsSelectable { get { return true; } }

        public override GameObject CreateElement()
        {
            var element = base.CreateElement();
            var textContainer = element.transform.GetChild(0).GetComponent<Text>();
            textContainer.text = ButtonText;
            return element;
        }
    }
}
