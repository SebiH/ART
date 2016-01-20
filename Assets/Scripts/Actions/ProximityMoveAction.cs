using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RSUnityToolkit;
using Assets.Code.Triggers;
using Assets.Code.Rules;

/// <summary>
/// Proximity move action: moves the game object in the direction of the trigger's translation data, 
/// if the gesture was inside a specified proximity zone
/// </summary>
public class ProximityMoveAction : VirtualWorldBoxAction
{

    #region Public Fields

    /// <summary>
    /// The translation sensitivity.
    /// </summary>
    public Vector3 Sensitivity = new Vector3(200, 200, 200);

    public float SmoothingFactor = 0;

    public SmoothingUtility.SmoothingTypes SmoothingType = SmoothingUtility.SmoothingTypes.Weighted;

    /// <summary>
    /// Determines the maximum distance between this GameObject and the gesture. If the gesture
    /// happens outside of this distance, the gesture will not be recognized.
    /// </summary>
    public float MaxActivationDistance = Mathf.Infinity;

    #endregion

    #region Private Fields

    private bool _actionTriggered = false;
    private SmoothingUtility _translationSmoothingUtility = new SmoothingUtility();

    #endregion

    #region Public Methods

    /// <summary>
    /// Determines whether this instance is support custom triggers.
    /// </summary>		
    public override bool IsSupportCustomTriggers()
    {
        return false;
    }

    /// <summary>
    /// Returns the actions's description for GUI purposes.
    /// </summary>
    /// <returns>
    /// The action description.
    /// </returns>
    public override string GetActionDescription()
    {
        return "This Action moves the game object in the direction of the trigger's translation data.";
    }

    /// <summary>
    /// Sets the default trigger values (for the triggers set in SetDefaultTriggers() )
    /// </summary>
    /// <param name='index'>
    /// Index of the trigger.
    /// </param>
    /// <param name='trigger'>
    /// A pointer to the trigger for which you can set the default rules.
    /// </param>
    public override void SetDefaultTriggerValues(int index, Trigger trigger)
    {
        switch (index)
        {
            case 0:
                trigger.FriendlyName = "Gesture Event";
                ((GestureTrackingTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<GestureTrackingRule>() };
                break;
        }
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Sets the default triggers for that action.
    /// </summary>
    protected override void SetDefaultTriggers()
    {
        SupportedTriggers = new Trigger[1]{
            AddHiddenComponent<GestureTrackingTrigger>(),
        };
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        updateVirtualWorldBoxCenter();

        ProcessAllTriggers();

        if (!SupportedTriggers[0].Success)
        {
            // TODO: better gesturemanager, see issue #22
            // workaround for now, since realsense's gesture do not seem to implement
            // (works only with the default pinch gesture probably)
            //var thumbs = GameObject.FindGameObjectsWithTag("thumb");
            //var indexFingers = GameObject.FindGameObjectsWithTag("index");
            //var gestureThreshold = 5f;

            //bool gesturePointFound = false;
            //Vector3 gesturePosition = Vector3.zero;

            //foreach (var thumb in thumbs)
            //{
            //    foreach (var indexFinger in indexFingers)
            //    {
            //        if ((thumb.transform.position - indexFinger.transform.position).magnitude < gestureThreshold)
            //        {
            //            // found cause of gesture
            //            gesturePointFound = true;
            //            gesturePosition = thumb.transform.position * 2 - indexFinger.transform.position;
            //            // for debugging
            //            Debug.Log("Found gesture!");

            //            break;
            //        }
            //    }

            //    if (gesturePointFound)
            //    {
            //        break;
            //    }
            //}





            //if (gesturePointFound && (transform.position - gesturePosition).magnitude < MaxActivationDistance)
            //{

            //}
            //else
            //{
            //    if (!gesturePointFound)
            //        Debug.Log("No GesturePoint Found");
            //    else
            //        Debug.Log("Outside of bounds!");
            //}
        }

        //if (!_actionTriggered)
        //{
        //    return;
        //}

        GestureTrackingTrigger trgr = (GestureTrackingTrigger)SupportedTriggers[0];

        if (trgr.Success)
        {
            Vector3 translate = trgr.Translation;
            translate.x = translate.x * Sensitivity.x;
            translate.y = translate.y * Sensitivity.y;
            translate.z = translate.z * Sensitivity.z;

            // smoothing:
            if (SmoothingFactor > 0)
            {
                translate = _translationSmoothingUtility.ProcessSmoothing(SmoothingType, SmoothingFactor, translate);
            }

            Vector3 vec = this.gameObject.transform.localPosition + translate;

            // Be sure we have valid values:				
            if (VirtualWorldBoxDimensions.x < 0)
            {
                VirtualWorldBoxDimensions.x = 0;
            }

            float left = VirtualWorldBoxCenter.x - (VirtualWorldBoxDimensions.x) / 2;
            float right = VirtualWorldBoxCenter.x + (VirtualWorldBoxDimensions.x) / 2;

            if (left > this.gameObject.transform.localPosition.x)
            {
                vec.x = left;
            }

            if (right < this.gameObject.transform.localPosition.x)
            {
                vec.x = right;
            }

            if (VirtualWorldBoxDimensions.y < 0)
            {
                VirtualWorldBoxDimensions.y = 0;
            }

            float top = VirtualWorldBoxCenter.y - (VirtualWorldBoxDimensions.y) / 2;
            float bottom = VirtualWorldBoxCenter.y + (VirtualWorldBoxDimensions.y) / 2;

            if (top > this.gameObject.transform.localPosition.y)
            {
                vec.y = top;
            }

            if (bottom < this.gameObject.transform.localPosition.y)
            {
                vec.y = bottom;
            }

            if (VirtualWorldBoxDimensions.z < 0)
            {
                VirtualWorldBoxDimensions.z = 0;
            }

            float back = VirtualWorldBoxCenter.z - (VirtualWorldBoxDimensions.z) / 2;
            float front = VirtualWorldBoxCenter.z + (VirtualWorldBoxDimensions.z) / 2;

            if (back > this.gameObject.transform.localPosition.z)
            {
                vec.z = back;
            }

            if (front < this.gameObject.transform.localPosition.z)
            {
                vec.z = front;
            }

            if (this.gameObject.transform.parent != null)
            {
                vec = this.gameObject.transform.parent.transform.TransformPoint(vec);
            }

            this.gameObject.GetComponent<Rigidbody>().MovePosition(vec);
        }
    }

    #endregion


    #region Debugging
    private void OnDrawGizmos()
    {
        if (!float.IsInfinity(MaxActivationDistance))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, MaxActivationDistance);
        }
    }
    #endregion
}
