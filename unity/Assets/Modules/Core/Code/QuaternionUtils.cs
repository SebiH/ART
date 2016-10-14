using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Core.Util
{
    public static class QuaternionUtils
    {
        public static Quaternion Average(IEnumerable<Quaternion> rotationlist)
        {
            if (rotationlist.Count() == 0)
            {
                return Quaternion.identity;
            }

            float x = 0;
            float y = 0;
            float z = 0;
            float w = 0;

            foreach (var quat in rotationlist)
            {
                x += quat.x;
                y += quat.y;
                z += quat.z;
                w += quat.w;
            }

            float k = 1.0f / Mathf.Sqrt(x * x + y * y + z * z + w * w);
            return new Quaternion(x * k, y * k, z * k, w * k);
        }
    }
}
