using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Menu
{
    public abstract class UIElement : MonoBehaviour
    {
        public Image Template;
        private Image _instance;

        public abstract bool IsSelectable { get; }

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
