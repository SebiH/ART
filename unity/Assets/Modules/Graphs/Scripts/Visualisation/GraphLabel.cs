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
    }
}
