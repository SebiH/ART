using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RSUnityToolkit;

/// <summary>
/// Activate action: Activate the game objects on trigger
/// Toggle action: Toggle game objects on trigger
/// </summary>
[EventTrigger.EventTriggerAtt]
public class ToggleAction : BaseAction
{

    #region public Fields	

    /// <summary>
    /// The game objects that will be toggled.
    /// </summary>
    public GameObject[] GameObjects;

    /// <summary>
    /// Time in seconds that has to pass before another toglge event can be triggered
    /// </summary>
    public float timeoutThreshold = 2;

    /// <summary>
    /// Determines the maximum distance between this GameObject and the gesture. If the gesture
    /// happens outside of this distance, the gesture will not be recognized.
    /// </summary>
    public float maxTriggerDistance = Mathf.Infinity;

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the actions's description for GUI purposes.
    /// </summary>
    /// <returns>
    /// The action description.
    /// </returns>
    public override string GetActionDescription()
    {
        return "This Action toggles the game objects whenever the associated triggers are fired.";
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
                ((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<GestureDetectedRule>() };
                ((GestureDetectedRule)(trigger.Rules[0])).Gesture = MCTTypes.RSUnityToolkitGestures.TwoFingersPinch;
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
        AddHiddenComponent<EventTrigger>()};
    }

    #endregion

    #region Private Methods

    private float lastActivationTime = 0;

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (GameObjects == null)
        {
            Debug.Log("No GameObjects set in Activate Action");
            return;
        }

        // don't trigger the action too often
        if (Time.time - lastActivationTime < timeoutThreshold)
        {
            return;
        }

        ProcessAllTriggers();

        foreach (Trigger trgr in SupportedTriggers)
        {
            if (trgr.Success)
            {
                // TODO: better gesturemanager, see issue #22
                // workaround for now, since realsense's gesture do not seem to implement
                // (works only with the default pinch gesture probably)
                var thumbs = GameObject.FindGameObjectsWithTag("thumb");
                var indexFingers = GameObject.FindGameObjectsWithTag("index");
                var gestureThreshold = 5f;

                bool gesturePointFound = false;
                Vector3 gesturePosition = Vector3.zero;

                foreach (var thumb in thumbs)
                {
                    foreach (var indexFinger in indexFingers)
                    {
                        if ((thumb.transform.position - indexFinger.transform.position).magnitude < gestureThreshold)
                        {
                            // found cause of gesture
                            gesturePointFound = true;
                            gesturePosition = thumb.transform.position * 2 - indexFinger.transform.position;
                            // for debugging
                            Debug.Log("Found gesture!");

                            break;
                        }
                    }

                    if (gesturePointFound)
                    {
                        break;
                    }
                }

                if (gesturePointFound && (transform.position - gesturePosition).magnitude < maxTriggerDistance)
                {
                    foreach (GameObject go in GameObjects)
                    {
                        go.SetActive(!go.activeInHierarchy);
                    }
                }
                else
                {
                    if (!gesturePointFound)
                        Debug.Log("No GesturePoint Found");
                    else
                        Debug.Log("Outside of bounds!");
                }

                lastActivationTime = Time.time;
                break;
            }
        }
    }

    #endregion


    #region Debugging
    private void OnDrawGizmos()
    {
        if (!float.IsInfinity(maxTriggerDistance))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, maxTriggerDistance);
        }
    }
    #endregion
}
