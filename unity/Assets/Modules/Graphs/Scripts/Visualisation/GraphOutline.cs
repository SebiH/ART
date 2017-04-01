using Assets.Modules.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Graphs.Visualisation
{
    public class GraphOutline : MonoBehaviour
    {
        public List<MeshRenderer> Borders = new List<MeshRenderer>();

        private Color32 _color;
        private Graph _graph;

        private void OnEnable()
        {
            _graph = UnityUtility.FindParent<Graph>(this);
        }

        private void Update()
        {
            if (_graph.Color != _color)
            {
                _color = _graph.Color;

                foreach (var border in Borders)
                {
                    border.material.color = _color;
                }
            }
        }
    }
}
