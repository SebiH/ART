using UnityEngine;

public class GravitySlider : SliderDemo
{
    private float _expectedGravity;

    protected override void Start()
    {
        base.Start();
        ChangeHandler += TESTOvrGainSlider_ChangeHandler;

        Physics.gravity = new Vector3(Physics.gravity.x, -1, Physics.gravity.z);
        _expectedGravity = -0.97f;
    }

    private void TESTOvrGainSlider_ChangeHandler(object sender, LMWidgets.EventArg<float> arg)
    {
        var fraction = GetSliderFraction();
        _expectedGravity = fraction * 2 - 0.97f; // Range from -1 to 1
        Physics.gravity = new Vector3(Physics.gravity.x, _expectedGravity, Physics.gravity.z);
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // set in each tick because it resets to 0 otherwise?
        SetPositionFromFraction((Physics.gravity.y + 0.97f) / 2);
    }



    public void SetGravityToSpace()
    {
        Physics.gravity = new Vector3(Physics.gravity.x, 0.03f, Physics.gravity.z);
    }

    public void SetGravityToInverted()
    {
        Physics.gravity = new Vector3(Physics.gravity.x, 1.03f, Physics.gravity.z);
    }
    
    public void SetGravityToNormal()
    {
        Physics.gravity = new Vector3(Physics.gravity.x, -0.97f, Physics.gravity.z);
    }
}
