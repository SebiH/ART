using Assets.Code.Triggers;
using RSUnityToolkit;
using UnityEngine;

namespace Assets.Code.Rules
{
    /// <summary>
    /// Tracks the positon of a gesture, if the gesture is detected
    /// </summary>
    public class GestureTrackingRule : BaseRule
    {
		/// <summary>
		/// The gesture to track
		/// </summary>
        public MCTTypes.RSUnityToolkitGestures Gesture;

        /// <summary>
        /// Which hand to track
        /// </summary>
        public PXCMHandData.AccessOrderType WhichHand = PXCMHandData.AccessOrderType.ACCESS_ORDER_NEAR_TO_FAR;

		/// <summary>
		/// The index of the hand.
		/// </summary>
		public int HandIndex = 0;

		/// <summary>
		/// the index of the hand can change once more hands are visible. Should continuously track the detected hand no matter its' index
		/// </summary>
		public bool ContinuousTracking = false;


        public GestureTrackingRule(): base()
        {
            FriendlyName = "Track Gesture";
        }



        override public string GetIconPath()
        {
            return @"RulesIcons/gesture-detected";
        }

        protected override bool OnRuleEnabled()
        {
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Hand);
            return true;

        }

        protected override void OnRuleDisabled()
        {
            SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.Hand);
        }

        public override string GetRuleDescription()
        {
            return "Fires upon gesture recognition";
        }

        private int _uniqueID;

        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.Hand))
            {
                trigger.ErrorDetected = true;
                Debug.LogError("Hand Analysis Module Not Set");
                return false;
            }

            if (!(trigger is GestureTrackingTrigger))
            {
                trigger.ErrorDetected = true;
                return false;
            }

            var trackingTrigger = (GestureTrackingTrigger)trigger;


            //AcquireFrame
            if (SenseToolkitManager.Instance.Initialized && SenseToolkitManager.Instance.HandDataOutput != null)
            {
                //if (SenseToolkitManager.Instance.HandDataOutput.QueryNumberOfHands() > 0)
                //{

                //    int totalNumOfFiredGestures = SenseToolkitManager.Instance.HandDataOutput.QueryFiredGesturesNumber();
                //    PXCMHandData.GestureData gestureData;

                //    for (int i = 0; i < totalNumOfFiredGestures; i++)
                //    {

                //        if (SenseToolkitManager.Instance.HandDataOutput.QueryFiredGestureData(i, out gestureData) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                //        {

                //            PXCMHandData.IHand handData = null;
                //            if ((ContinuousTracking && SenseToolkitManager.Instance.HandDataOutput.QueryHandDataById(_uniqueID, out handData) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                //                    ||
                //                    SenseToolkitManager.Instance.HandDataOutput.QueryHandData(WhichHand, HandIndex, out handData) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                //            {
                //                _uniqueID = handData.QueryUniqueId();

                //                if (handData.QueryUniqueId() == gestureData.handId)
                //                {
                //                    MCTTypes.RSUnityToolkitGestures firedGesture = MCTTypes.GetGesture(gestureData.name);

                //                    if (((!Gesture.Equals(MCTTypes.RSUnityToolkitGestures.None)) && Gesture.Equals(firedGesture)))
                //                    {
                //                        trackingTrigger.IsActive = true;
                //                        trackingTrigger.TimeOfLastDetection = Time.time;

                //                        // TODO: track position based on gesture, not on index finger!
                //                        PXCMHandData.JointData jointData;
                //                        handData.QueryTrackedJoint(PXCMHandData.JointType.JOINT_INDEX_TIP, out jointData);
                //                        trackingTrigger.Translation = jointData.positionWorld;
                //                        trackingTrigger.Rotation = jointData.positionImage;

                //                        Debug.Log("Detected gesture!");

                //                        return true;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                PXCMHandData.GestureData gestureData;
                if (SenseToolkitManager.Instance.HandDataOutput.IsGestureFired("two_fingers_pinch_open", out gestureData))
                {
                    trackingTrigger.IsActive = true;
                    trackingTrigger.TimeOfLastDetection = Time.time;


                    PXCMHandData.IHand handData = null;
                    SenseToolkitManager.Instance.HandDataOutput.QueryHandData(WhichHand, HandIndex, out handData);

                    PXCMHandData.JointData jointData;
                    handData.QueryTrackedJoint(PXCMHandData.JointType.JOINT_INDEX_TIP, out jointData);
                    trackingTrigger.Translation = jointData.positionWorld;
                    trackingTrigger.Rotation = jointData.positionImage;

                    Debug.Log("Found gesture");

                    return true;
                }

            }

            var gestureTimeoutThreshold = 0.5f;
            if (trackingTrigger.IsActive && Mathf.Abs(Time.time - trackingTrigger.TimeOfLastDetection) <= gestureTimeoutThreshold)
            {
                Debug.Log("Lost Gesture, but still within threshld");
                return true;
            }

            trackingTrigger.IsActive = false;
            return false;
        }
    }
}
