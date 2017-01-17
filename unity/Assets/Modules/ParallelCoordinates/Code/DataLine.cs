using Assets.Modules.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    /// <summary>
    /// Collects line segments generated by line generator which belong to the same data index, for easier operations on all segments
    /// </summary>
    public class DataLine
    {
        private int _dataIndex = -1;
        public int DataIndex { get { return _dataIndex; } }

        private List<LineSegment> _lineSegments = new List<LineSegment>();

        private Color _color;

        public DataLine(int dataIndex)
        {
            _dataIndex = dataIndex;
            _color = Theme.GetColor(dataIndex);
        }

        public void AddSegment(LineSegment segment)
        {
            _lineSegments.Add(segment);
            segment.SetColor(_color);
        }

        public void RemoveSegment(LineSegment segment)
        {
            _lineSegments.Remove(segment);
        }

        public void SetHighlight(bool isHighlighted)
        {
            foreach (var segment in _lineSegments)
            {
                if (isHighlighted)
                {
                    segment.SetColor(_color);
                }
                else
                {
                    segment.SetColor(Color.gray);
                }
            }
        }
    }
}
