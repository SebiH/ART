using System.Collections;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class LineSegment
    {
        public Vector3 Start;
        public Vector3 End = new Vector3(0, 0, 1);

        public Color32 Color = new Color32(25, 118, 210, 255);
        public bool IsFiltered;


        public int MeshIndex = -1;

        private GraphicsLineRenderer _renderer;

        public void SetRenderer(GraphicsLineRenderer renderer)
        {
            Debug.Assert(_renderer == null, "Cannot reassign LineSegment to different renderer!");
            _renderer = renderer;
            _renderer.AddLine(this);
        }

        public IEnumerator AnimateStart(Vector3 destStart)
        {
            var origStart = Start;
            var totalDeltaTime = 0.0f;
            Vector3 currentStart = Start;

            while (totalDeltaTime < 1.0f)
            {
                totalDeltaTime += Time.deltaTime / 10f;
                currentStart = Vector3.Lerp(origStart, destStart, totalDeltaTime);
                Start = currentStart;

                if (MeshIndex >= 0) { _renderer.UpdateLine(this); }

                yield return new WaitForEndOfFrame();
            }

            Start = destStart;
        }

        public IEnumerator AnimateEnd(Vector3 destEnd)
        {
            var origEnd = End;
            var totalDeltaTime = 0.0f;
            Vector3 currentEnd = End;

            while (totalDeltaTime < 1.0f)
            {
                totalDeltaTime += Time.deltaTime / 10f;
                currentEnd = Vector3.Lerp(origEnd, destEnd, totalDeltaTime);
                End = currentEnd;

                if (MeshIndex >= 0) { _renderer.UpdateLine(this); }

                yield return new WaitForEndOfFrame();
            }

            End = destEnd;
        }

        public void UpdateVisual()
        {
            if (MeshIndex >= 0)
            {
                _renderer.UpdateLine(this);
            }
        }
    }
}
