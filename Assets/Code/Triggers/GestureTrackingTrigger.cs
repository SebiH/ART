using RSUnityToolkit;
using UnityEngine;

namespace Assets.Code.Triggers
{
    [AddComponentMenu("")]
    public class GestureTrackingTrigger : Trigger
    {

        public class GestureTrackingTriggerAtt : TriggerAtt
        {
        }

        public bool IsActive = false;
        public float TimeOfLastDetection = 0f;

        public Vector3 Translation { get; set; }
        public Vector3 Rotation { get; set; }


        protected override string GetAttributeName()
        {
            return typeof(GestureTrackingTriggerAtt).Name;
        }

        protected override string GetFriendlyName()
        {
            return "Gesture Tracking Trigger";
        }



    }
}
