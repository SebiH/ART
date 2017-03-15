using UnityEngine;

namespace Assets.Modules.Core.Animations
{
    public class QuaternionAnimation : ProceduralAnimation<Quaternion>
    {
        public QuaternionAnimation(float animationSpeed) : base(animationSpeed)
        {
        }

        protected override Quaternion Lerp(Quaternion start, Quaternion end, float weight)
        {
            return Quaternion.Lerp(start, end, weight);
        }
    }
}
