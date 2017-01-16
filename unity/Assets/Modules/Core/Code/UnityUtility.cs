using UnityEngine;

namespace Assets.Modules.Core
{
    public static class UnityUtility
    {
        public static T FindParent<T>(this MonoBehaviour start)
            where T : MonoBehaviour
        {
            var curr = start.transform;
            while (curr && curr.GetComponent<T>() == null)
            {
                curr = curr.parent;
            }

            return curr ? curr.GetComponent<T>() : null;
        }

    }
}
