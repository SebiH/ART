using UnityEngine;

namespace Assets.Modules.Core.Animations
{
    public class ColorsAnimation : ProceduralAnimation<Color32[]>
    {
        public ColorsAnimation(float animationSpeed) : base(animationSpeed)
        {
        }

        protected override Color32[] Lerp(Color32[] start, Color32[] end, float weight)
        {
            var colors = new Color32[start.Length];

            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Color32.Lerp(start[i], end[i], weight);
            }

            return colors;
        }
    }
}
