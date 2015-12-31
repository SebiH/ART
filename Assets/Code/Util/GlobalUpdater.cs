using UnityEngine;


namespace Assets.Code.Util
{
    public delegate void OnUpdateHandler();

    /// <summary>
    /// Utility to add dynamic actions to the update event, e.g. for performing an animation.
    /// Needs to bound to exactly *one* GameObject per scene!
    /// (Otherwise handlers will be invoked multiple times per update)
    /// </summary>
    public class GlobalUpdater : MonoBehaviour
    {
        public static event OnUpdateHandler OnUpdate;

        void Update()
        {
            if (OnUpdate != null)
            {
                OnUpdate();
            }
        }
    }

}
