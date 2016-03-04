using UnityEngine;

public class GravitySlider : SliderDemo
{
    private float _expectedGravity;

    protected override void Start()
    {
        base.Start();
        ChangeHandler += TESTOvrGainSlider_ChangeHandler;

        Physics.gravity = new Vector3(Physics.gravity.x, -1, Physics.gravity.z);
        _expectedGravity = -1;
    }

    private void TESTOvrGainSlider_ChangeHandler(object sender, LMWidgets.EventArg<float> arg)
    {
        var fraction = GetSliderFraction();
        _expectedGravity = fraction * 4 - 2; // Range from -2 to 2
        Physics.gravity = new Vector3(Physics.gravity.x, _expectedGravity, Physics.gravity.z);
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // set in each tick because it resets to 0 otherwise?
        SetPositionFromFraction((Physics.gravity.y + 2) / 4);
    }
}
