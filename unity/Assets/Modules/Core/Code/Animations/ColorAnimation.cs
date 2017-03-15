using System;
using UnityEngine;

namespace Assets.Modules.Core.Animations
{
    public class ColorAnimation : ProceduralAnimation<Color32>
    {
        public ColorAnimation(float animationSpeed) : base(animationSpeed)
        {
        }

        protected override Color32 Lerp(Color32 start, Color32 end, float weight)
        {
            return Color32.Lerp(start, end, weight);
        }
    }
}
