using UnityEngine;

namespace Assets.Modules.Graph
{
    public class BarDataPoint : DataPointOld
    {
        protected override void OnHeightChange(float height)
        {
            var currPos = transform.localPosition;
            transform.localPosition = new Vector3(currPos.x, height / 2, currPos.z);
            transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
        }


        protected override void OnHighlightChange(bool isHighlighted)
        {
            var renderer = GetComponent<MeshRenderer>();
            var color = isHighlighted ? Color.yellow : Color.white;
            renderer.material.color = color;
        }
    }
}
