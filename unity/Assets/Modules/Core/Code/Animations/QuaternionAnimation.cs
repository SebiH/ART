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
            if (Mathf.Abs(start.x) < Mathf.Epsilon && Mathf.Abs(start.y) < Mathf.Epsilon && Mathf.Abs(start.z) < Mathf.Epsilon && Mathf.Abs(start.w) < Mathf.Epsilon)
                return Quaternion.identity;
            if (Mathf.Abs(end.x) < Mathf.Epsilon && Mathf.Abs(end.y) < Mathf.Epsilon && Mathf.Abs(end.z) < Mathf.Epsilon && Mathf.Abs(end.w) < Mathf.Epsilon)
                return Quaternion.identity;

            return Quaternion.Lerp(start, end, weight);
        }
    }
}
