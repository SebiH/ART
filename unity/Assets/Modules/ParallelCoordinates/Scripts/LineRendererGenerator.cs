using System;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class LineRendererGenerator : LineGenerator
    {
        // must have LineRenderer
        public GameObject LineTemplate;

        void OnEnable()
        {
        }

        void OnDisable()
        {
            ClearLines();
        }

        public override void GenerateLines(Vector2[] startData, Vector2[] endData)
        {
            ClearLines();

            for (int i = 0; i < startData.Length; i++)
            {
                var go = Instantiate(LineTemplate);
                go.transform.parent = transform;
                go.hideFlags = HideFlags.HideAndDontSave;

                var lr = go.GetComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.numPositions = 2;
                lr.SetPosition(0, transform.TransformPoint(startData[i].x, startData[i].y, 0));
                lr.SetPosition(1, transform.TransformPoint(endData[i].x, endData[i].y, 1));
            }
        }

        public override void SetStart(Vector2[] startData)
        {
            Debug.LogWarning("NYI");
        }

        public override void SetEnd(Vector2[] endData)
        {
            Debug.LogWarning("NYI");
        }

        private void ClearLines()
        {
            for (int i = transform.childCount; i > 0; i--)
            {
                var child = transform.GetChild(i - 1);
                Destroy(child.gameObject);
            }
        }
    }
}
