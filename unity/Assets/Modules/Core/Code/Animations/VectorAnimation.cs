using UnityEngine;

namespace Assets.Modules.Core.Animations
{
    public class VectorAnimation : ProceduralAnimation<Vector3>
    {
        public VectorAnimation(float animationSpeed) : base(animationSpeed)
        {
        }

        protected override Vector3 Lerp(Vector3 start, Vector3 end, float weight)
        {
            return Vector3.Lerp(start, end, weight);
        }
    }
}
