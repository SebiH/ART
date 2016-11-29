using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class RandomLineColour : MonoBehaviour
    {
        void OnEnable()
        {
            var lr = GetComponent<LineRenderer>();
            lr.material.color = Random.ColorHSV(0, 1, 1, 1);
        }
    }
}
