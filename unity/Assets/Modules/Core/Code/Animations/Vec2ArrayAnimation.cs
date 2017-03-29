using UnityEngine;

namespace Assets.Modules.Core.Animations
{
    public class Vec2ArrayAnimation : ProceduralAnimation<Vector2[]>
    {
        public Vec2ArrayAnimation(float animationSpeed) : base(animationSpeed)
        {
        }

        protected override Vector2[] Lerp(Vector2[] start, Vector2[] end, float weight)
        {
            var current = new Vector2[start.Length];

            for (var i = 0; i < current.Length; i++)
            {
                current[i] = Vector2.Lerp(start[i], end[i], weight);
            }

            return current;
        }
    }
}
