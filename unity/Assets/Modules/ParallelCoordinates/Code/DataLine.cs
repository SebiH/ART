using Assets.Modules.Core.Animations;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    /// <summary>
    /// Collects line segments generated by line generator which belong to the same data index, for easier operations on all segments
    /// </summary>
    public class DataLine
    {
        const byte TRANSPARENCY_FILTERED = 10;
        const byte TRANSPARENCY_NORMAL = 255;
        const float ANIMATION_SPEED = 1f; // in seconds

        public int DataIndex { get; private set; }
        public int SegmentCount { get { return _lineSegments.Count; } }

        private bool _isFiltered = false;
        public bool IsFiltered
        {
            get { return _isFiltered; }
            set
            {
                if (_isFiltered != value)
                {
                    _isFiltered = value;

                    foreach (var segment in _lineSegments)
                    {
                        segment.Transparency = _isFiltered ? TRANSPARENCY_FILTERED : TRANSPARENCY_NORMAL;
                    }
                }
            }
        }

        private Color32 _color = new Color32(255, 255, 255, 255);
        private ColorAnimation _colorAnimation = new ColorAnimation(ANIMATION_SPEED);
        public Color32 Color
        {
            get { return _color; }
            set
            {
                if (_color.r != value.r && _color.g != value.g && _color.b != value.b)
                {
                    _colorAnimation.Restart(_color, value);
                    _color = value;
                }
            }
        }

        private List<LineSegment> _lineSegments = new List<LineSegment>();

        public DataLine(int dataIndex)
        {
            DataIndex = dataIndex;
            _colorAnimation.Init(_color);

            _colorAnimation.Update += (val) =>
            {
                foreach (var segment in _lineSegments)
                {
                    segment.Color = val;
                }
            };
        }

        public void AddSegment(LineSegment segment)
        {
            _lineSegments.Add(segment);
            segment.Color = _colorAnimation.CurrentValue;
            segment.Transparency = _isFiltered ? TRANSPARENCY_FILTERED : TRANSPARENCY_NORMAL;
        }

        public void RemoveSegment(LineSegment segment)
        {
            _lineSegments.Remove(segment);
        }
    }
}
