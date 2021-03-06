using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(ParallelCoordinatesManager))]
    public class Debug_ColorSetter : MonoBehaviour
    {
        public bool DoUpdate = false;

        [Range(0f, 1f)]
        public float FilterPercent = 0.3f;
        public int LineCount = 100;

        private Color32[] _colors = new[]
        {
            new Color32(244, 67, 54, 255), // red
            new Color32(33, 150, 243, 255), // blue
            new Color32(255, 152, 0, 255), // orange
            new Color32(76, 175, 80, 255) // green
        };

        private void OnEnable()
        {
        }

        private void LateUpdate()
        {
            if (DoUpdate)
            {
                Randomize();
                DoUpdate = false;
            }
        }

        public void Randomize()
        {
            var manager = GetComponent<ParallelCoordinatesManager>();
            var colors = new Color32[LineCount];

            for (var i = 0; i < colors.Length; i++)
            {
                var isFiltered = Random.value < FilterPercent;
                var col = _colors[Random.Range(0, _colors.Length)];
                col.a = (byte)(isFiltered ? 60 : 255);

                colors[i] = col;
            }

            manager.SetColors(colors);
        }
    }
}
