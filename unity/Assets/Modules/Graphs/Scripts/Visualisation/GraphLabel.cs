using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Graphs
{
    public class GraphLabel : MonoBehaviour
    {
        public Text Front;
        public Text Back;

        private void OnEnable()
        {
            var text = "Here be dragons";
            Front.text = text;
            Back.text = text;
        }
    }
}
