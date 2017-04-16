using Assets.Modules.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Graphs.Visualisation
{
    [RequireComponent(typeof(Image))]
    public class GraphFlippedIcon : MonoBehaviour
    {
        private Graph _graph;
        private Image _image;

        private void OnEnable()
        {
            _graph = UnityUtility.FindParent<Graph>(this);
            _image = GetComponent<Image>();
            _image.material = Instantiate(_image.material);
        }

        private void Update()
        {
            _image.enabled = _graph.IsFlipped;
        }
    }
}
