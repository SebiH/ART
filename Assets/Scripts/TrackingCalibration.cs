using UnityEngine;
using System.Linq;
using Assets.Code.Console;

public class TrackingCalibration : MonoBehaviour {

    private int currentStep;
    private Vector3[] positions = new []
    {
        new Vector3(0, 0 , 0),
        new Vector3(10, 0 , 0),
        new Vector3(10, 10 , 0),
        new Vector3(0, 10 , 0),
    };    

	void Start ()
    {
        currentStep = 0;

        UCommandRegister.RegisterCommand(new UConsoleCommand("resetCalibration", (args) =>
        {
            currentStep = 0;
            return "Done!";
        }));

        UCommandRegister.RegisterCommand(new UConsoleCommand("setCalibrationStep", (args) =>
        {
            if (args.Count() < 1)
            {
                return "Not enough arguments!";
            }

            activateStep(int.Parse(args.First()));

            return "Done!";
        }));
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Space))
        {
            activateStep(currentStep + 1);
        }
	}

    private void activateStep(int step)
    {
        if (step >= 0 && step < positions.Length)
        {
            currentStep = step;
            transform.position = positions[step];
        }
    }
}
