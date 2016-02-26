using UnityEngine;
using System.Collections;
using LMWidgets;

public class TESTOvrGainSlider : SliderDemo
{
    public GameObject OvrCamera;
    private Ovrvision OvrScript;

    protected override void Start()
    {
        base.Start();
        ChangeHandler += TESTOvrGainSlider_ChangeHandler;
        OvrScript = OvrCamera.GetComponent<Ovrvision>();
    }

    private void TESTOvrGainSlider_ChangeHandler(object sender, LMWidgets.EventArg<float> arg)
    {
        var fraction = GetSliderFraction();
        OvrScript.conf_gain = (int)(Mathf.Floor(fraction * 47));
        OvrScript.UpdateOvrvisionSetting();
    }

}
