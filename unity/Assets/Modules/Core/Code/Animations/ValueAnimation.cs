using UnityEngine;

namespace Assets.Modules.Core.Animations
{
    public class ValueAnimation : ProceduralAnimation<float>
    {
        public ValueAnimation(float animationSpeed) : base(animationSpeed)
        {
        }

        protected override float Lerp(float start, float end, float weight)
        {
            return Mathf.Lerp(start, end, weight);
        }
    }
}
