using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class GraphVisualisation : MonoBehaviour
    {
        public GraphLabel LabelX;
        public GraphLabel LabelY;
        public GraphDataField DataField;
        public GameObject FilterSymbol;

        public void SetDimensionX(string name, float domainMin, float domainMax)
        {
            if (LabelX)
            {
                LabelX.Text = name;
            }
        }

        public void SetDimensionY(string name, float domainMin, float domainMax)
        {
            if (LabelY)
            {
                LabelY.Text = name;
            }
        }

        public void SetFilterActive(bool status)
        {
            if (FilterSymbol)
            {
                FilterSymbol.SetActive(status);
            }
        }
    }
}
