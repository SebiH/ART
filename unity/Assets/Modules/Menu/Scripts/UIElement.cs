using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Menu
{
    public class UIElement : MonoBehaviour
    {
        public Text TextContainer;
        public Image Panel;
        public Color HighlightColor;

        private Color _baseColor;

        void OnEnable()
        {
            _baseColor = Panel.color;
        }

        public void SetHighlight(bool isHighlighted)
        {
            if (isHighlighted)
            {
                Panel.color = HighlightColor;
            }
            else
            {
                Panel.color = _baseColor;
            }
        }

        public void SetText(string text)
        {
            TextContainer.text = text;
        }
    }
}
