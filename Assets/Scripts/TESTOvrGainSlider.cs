using UnityEngine;
using System.Collections;
using LMWidgets;

public class TESTOvrGainSlider : SliderDemo
{
    public GameObject OvrCamera;
    private Ovrvision _ovrScript;
    private int _expectedConfidence;
    private bool _hasSliderChanged;

    protected override void Start()
    {
        base.Start();
        ChangeHandler += TESTOvrGainSlider_ChangeHandler;
        _ovrScript = OvrCamera.GetComponent<Ovrvision>();
    }

    private void TESTOvrGainSlider_ChangeHandler(object sender, LMWidgets.EventArg<float> arg)
    {
        var fraction = GetSliderFraction();
        _expectedConfidence = (int)(Mathf.Floor(fraction * 47));
        _hasSliderChanged = true;
    }


    private float _timeSinceLastUpdate;
    private float _minTimeBetweenUpdates = 0.5f;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // only change the actual value if the change came from this script
        if (!_hasSliderChanged)
        {
            // if the change didn't come from this script, make sure we're updating
            // the slider to the current value
            _expectedConfidence = _ovrScript.conf_gain;
            SetPositionFromFraction(_expectedConfidence / 47f);
            return;
        }

        _timeSinceLastUpdate += Time.deltaTime;

        if (_timeSinceLastUpdate > _minTimeBetweenUpdates)
        {
            _ovrScript.conf_gain = _expectedConfidence;
            _ovrScript.UpdateOvrvisionSetting();
            _hasSliderChanged = false;
            _timeSinceLastUpdate = 0;
        }
    }

}
