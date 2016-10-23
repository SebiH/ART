using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Menu
{
    public abstract class UIElement : MonoBehaviour
    {
        public Image Template;
        private Image _instance;

        public bool IsSelected { get; protected set; }

        public abstract bool IsSelectable { get; }
        public abstract void HandleInput(InputType input);

        public virtual GameObject CreateElement()
        {
            _instance = Instantiate(Template);
            return _instance.gameObject;
        }

        public virtual void SetHighlight(bool isHighlighted, Color highlightColor)
        {
            IsSelected = isHighlighted;
            if (isHighlighted)
            {
                _instance.color = highlightColor;
            }
            else
            {
                _instance.color = Template.color;
            }
        }



        void OnEnable()
        {
            Menu.Instance.MInput.OnButtonPress += OnInput;
        }

        void OnDisable()
        {
            Menu.Instance.MInput.OnButtonPress -= OnInput;
        }

        private void OnInput(InputType input)
        {
            if (IsSelected)
            {
                HandleInput(input);
            }
        }
    }
}
