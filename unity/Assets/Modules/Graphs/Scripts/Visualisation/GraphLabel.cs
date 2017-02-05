using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Graphs
{
    public class GraphLabel : MonoBehaviour
    {
        public Text Label;

        private void OnEnable()
        {
            Label.text = "Here be dragons";
        }
    }
}
