using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(Rigidbody))]
    public class LineSelector : MonoBehaviour
    {
        private ParallelCoordinatesVisualisation _activeVisualisation;
        private Color32[] _colors;
        private int _prevNearestLine = -1;

        private void OnTriggerEnter(Collider other)
        {
            _activeVisualisation = other.GetComponentInParent<ParallelCoordinatesVisualisation>();
        }

        //private void OnDrawGizmos()
        private void Update()
        {
            if (_activeVisualisation && _activeVisualisation.Left && _activeVisualisation.Right)
            {
                var leftGraph = _activeVisualisation.Left;
                var rightGraph = _activeVisualisation.Right;

                if (leftGraph.Graph.DimX != null && leftGraph.Graph.DimY != null && rightGraph.Graph.DimX != null && rightGraph.Graph.DimY != null)
                {
                    var nearestLine = GetNearestLine(leftGraph, rightGraph);

                    if (_colors == null && leftGraph.Graph.DimX.Data != null)
                    {
                        _colors = new Color32[leftGraph.Graph.DimX.Data.Length];
                    }

                    if (nearestLine != _prevNearestLine)
                    {
                        if (_prevNearestLine >= 0)
                        {
                            _colors[_prevNearestLine] = new Color32(255, 255, 255, 255);
                        }

                        if (nearestLine >= 0)
                        {
                            if (_prevNearestLine < 0)
                            {
                                for (var i = 0; i < _colors.Length; i++)
                                {
                                    _colors[i].a = 60;
                                }
                            }
                            else
                            {
                                _colors[_prevNearestLine].a = 60;
                                _colors[_prevNearestLine].r = 255;
                                _colors[_prevNearestLine].g = 255;
                                _colors[_prevNearestLine].b = 255;
                            }

                            _colors[nearestLine].a = 255;
                            _colors[nearestLine].r = 255;
                            _colors[nearestLine].g = 0;
                            _colors[nearestLine].b = 0;
                        }
                        else
                        {
                            for (var i = 0; i < _colors.Length; i++)
                            {
                                _colors[i] = new Color32(255, 255, 255, 255);
                            }
                        }
                        ParallelCoordinatesManager.Instance.SetColors(_colors);

                        _prevNearestLine = nearestLine;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var vis = other.GetComponentInParent<ParallelCoordinatesVisualisation>();
            if (_activeVisualisation == vis)
            {
                _activeVisualisation = null;
            }
        }



        private int GetNearestLine(GraphMetaData left, GraphMetaData right)
        {
            var dataXL = left.Graph.DimX.ScaledData;
            var dataYL = left.Graph.DimY.ScaledData;
            var dataXR = right.Graph.DimX.ScaledData;
            var dataYR = right.Graph.DimY.ScaledData;

            Debug.Assert(dataXL.Length == dataYL.Length && dataYL.Length == dataYR.Length && dataYR.Length == dataXR.Length);

            var gapTotal = left.Graph.transform.InverseTransformPoint(right.Graph.transform.position).z;
            var gapPercent = left.Graph.transform.InverseTransformPoint(transform.position).z / gapTotal;

            var nearestLine = -1;
            var nearestDistance = -1f;
            for (var i = 0; i < left.Graph.DimX.ScaledData.Length; i++) 
            {
                var startLocal = new Vector3(dataXL[i], dataYL[i], 0);
                var startWorld = left.Visualisation.transform.TransformPoint(startLocal);

                var endLocal = new Vector3(dataXR[i], dataYR[i], 0);
                var endWorld = right.Visualisation.transform.TransformPoint(endLocal);

                var linePos = startWorld + gapPercent * (endWorld - startWorld);

                var distance = Mathf.Abs((transform.position - linePos).magnitude);
                if (nearestDistance > distance || nearestDistance < 0)
                {
                    nearestDistance = distance;
                    nearestLine = i;
                }

                //Gizmos.color = Color.red;
                //Gizmos.DrawCube(linePos, Vector3.one * 0.01f);
            }

            //if (nearestLine >= 0)
            //{
            //    var startLocal = new Vector3(dataXL[nearestLine], dataYL[nearestLine], 0);
            //    var startWorld = left.Visualisation.transform.TransformPoint(startLocal);

            //    var endLocal = new Vector3(dataXR[nearestLine], dataYR[nearestLine], 0);
            //    var endWorld = right.Visualisation.transform.TransformPoint(endLocal);

            //    Gizmos.color = Color.blue;
            //    Gizmos.DrawLine(startWorld, endWorld);
            //}

            return nearestLine;
        }
    }
}
