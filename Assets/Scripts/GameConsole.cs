using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Code.Console;
using System.Text;

public class GameConsole : MonoBehaviour {
    public Text logComponent;
    public InputField commandComponent;
    private UConsole console;

	void Start ()
    {
        console = new UConsole();
	}

    void OnDestroy()
    {
        // TODO?
    }


    void Update()
    {
        foreach (char c in Input.inputString)
        {
            console.CurrentInput += c;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            console.ExecuteCommand();
        }
        
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            console.CurrentInput = console.CurrentInput.Substring(0, console.CurrentInput.Length - 2);
        }

        StringBuilder sb = new StringBuilder();
        foreach (var logline in console.Log)
        {
            sb.AppendLine(logline);
        }

        logComponent.text = sb.ToString();

        commandComponent.text = console.CurrentInput;
    }
	
}
