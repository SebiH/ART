using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class LineGenerator : MonoBehaviour
    {
        // must have LineSegment
        public GameObject LineTemplate;

        private LineSegment[] _lineSegments;

        void OnEnable()
        {
        }

        void OnDisable()
        {
            ClearLines();
        }

        public void GenerateLines(Vector2[] startData, Vector2[] endData)
        {
            Debug.Assert(startData.Length == endData.Length);

            ClearLines();
            _lineSegments = new LineSegment[startData.Length];

            for (int i = 0; i < startData.Length; i++)
            {
                var go = Instantiate(LineTemplate);
                go.transform.parent = transform;
                // reduce editor load (?)
                go.hideFlags = HideFlags.HideAndDontSave;

                var segment = go.GetComponent<LineSegment>();
                segment.SetPositions(transform.TransformPoint(startData[i].x, startData[i].y, 0), transform.TransformPoint(endData[i].x, endData[i].y, 1));

                ContinualLines.Get(i).AddSegment(segment);
                _lineSegments[i] = segment;
            }
        }

        public void SetStart(Vector2[] startData)
        {
            if (startData.Length != _lineSegments.Length)
            {
                // TODO: handle edge case? shouldn't happen with given data
                Debug.LogWarning("LineGenerator segment size doesn't match!");
                return;
            }

            // TODO: animate
            for (int i = 0; i < startData.Length; i++)
            {
                _lineSegments[i].SetStartAnimated(transform.TransformPoint(startData[i].x, startData[i].y, 0));
            }
        }

        public void SetEnd(Vector2[] endData)
        {
            if (endData.Length != _lineSegments.Length)
            {
                // TODO: handle edge case? shouldn't happen with given data
                Debug.LogWarning("LineGenerator segment size doesn't match!");
                return;
            }

            // TODO: animate
            for (int i = 0; i < endData.Length; i++)
            {
                _lineSegments[i].SetEndAnimated(transform.TransformPoint(endData[i].x, endData[i].y, 1));
            }
        }

        private void ClearLines()
        {
            if (_lineSegments != null)
            {
                for (int i = 0; i < _lineSegments.Length; i++)
                {
                    ContinualLines.Get(i).RemoveSegment(_lineSegments[i]);
                    var go = _lineSegments[i].gameObject;
                    Destroy(go);
                }

                _lineSegments = null;
            }
        }
    }
}
