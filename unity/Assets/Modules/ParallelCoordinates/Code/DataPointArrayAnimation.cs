using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class DataPointArrayAnimation : ProceduralAnimation<Graph.DataPoint[]>
    {
        public DataPointArrayAnimation(float animationSpeed) : base(animationSpeed)
        {
        }

        protected override Graph.DataPoint[] Lerp(Graph.DataPoint[] start, Graph.DataPoint[] end, float weight)
        {
            if (start.Length != end.Length)
            {
                return end;
            }

            var current = new Graph.DataPoint[start.Length];

            for (var i = 0; i < current.Length; i++)
            {
                current[i].Pos = Vector2.Lerp(start[i].Pos, end[i].Pos, weight);
                current[i].IsNull = end[i].IsNull;
            }

            return current;
        }
    }
}
