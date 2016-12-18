using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class LineGenerator : MonoBehaviour
    {
        // must have LineRenderer
        public GameObject LineTemplate;

        private LineRenderer[] _lineRenderers;

        void OnEnable()
        {
        }

        void OnDisable()
        {
            ClearLines();
        }

        public void GenerateLines(Vector2[] startData, Vector2[] endData)
        {
            ClearLines();

            for (int i = 0; i < startData.Length; i++)
            {
                var go = Instantiate(LineTemplate);
                go.transform.parent = transform;
                go.hideFlags = HideFlags.HideAndDontSave;

                var lr = go.GetComponent<LineRenderer>();
                lr.material.color = Theme.GetColor(i);
                lr.useWorldSpace = false;
                lr.numPositions = 2;
                lr.SetPosition(0, transform.TransformPoint(startData[i].x, startData[i].y, 0));
                lr.SetPosition(1, transform.TransformPoint(endData[i].x, endData[i].y, 1));
            }
        }

        public void SetStart(Vector2[] startData)
        {
            if (startData.Length != _lineRenderers.Length)
            {
                // TODO: handle edge case? shouldn't happen with given data
                Debug.LogWarning("Size doesn't match!");
                return;
            }

            // TODO: animate
            for (int i = 0; i < startData.Length; i++)
            {
                _lineRenderers[i].SetPosition(0, transform.TransformPoint(startData[i].x, startData[i].y, 0));
            }
        }

        public void SetEnd(Vector2[] endData)
        {
            if (endData.Length != _lineRenderers.Length)
            {
                // TODO: handle edge case? shouldn't happen with given data
                Debug.LogWarning("Size doesn't match!");
                return;
            }

            // TODO: animate
            for (int i = 0; i < endData.Length; i++)
            {
                _lineRenderers[i].SetPosition(1, transform.TransformPoint(endData[i].x, endData[i].y, 1));
            }
        }

        private void ClearLines()
        {
            for (int i = transform.childCount; i > 0; i--)
            {
                var child = transform.GetChild(i - 1);
                Destroy(child.gameObject);
            }

            _lineRenderers = null;
        }
    }
}
