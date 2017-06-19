using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Graphs.Visualisation
{
    public class GraphLabel : MonoBehaviour
    {
        public Text Front;
        public Text Back;

        public string Text
        {
            set { Front.text = value; Back.text = value; }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    Front.enabled = _isVisible;
                    Back.enabled = _isVisible;
                }
            }
        }
    }
}
