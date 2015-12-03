using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Code.Console;
using System.Text;
using System.Collections.Generic;
using Assets;
using System;

public class GameConsole : MonoBehaviour {
    public Text logComponent;
    public InputField commandComponent;
    private UConsole console;

    private UConsoleCommand helpCmd;
	void Start ()
    {
        console = new UConsole();
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
            if (console.CurrentInput.Length > 2)
            {
                console.CurrentInput = console.CurrentInput.Substring(0, console.CurrentInput.Length - 2);
            }
            else
            {
                console.CurrentInput = "";
            }
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
