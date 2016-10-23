using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Menu
{
    public class UIElement : MonoBehaviour
    {
        public bool IsSelectable;
        public Image Template;
        private Image _instance;

        public virtual GameObject CreateElement()
        {
            _instance = Instantiate(Template);
            return _instance.gameObject;
        }

        public virtual void SetHighlight(bool isHighlighted, Color highlightColor)
        {
            if (isHighlighted)
            {
                _instance.color = highlightColor;
            }
            else
            {
                _instance.color = Template.color;
            }
        }
    }
}
