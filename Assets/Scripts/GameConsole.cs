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

        helpCmd = new UConsoleCommand("help", new Func<IEnumerable<string>, string>((args) =>
        {
            return "Hello, console!";
        }));

        UCommandRegister.RegisterCommand(helpCmd);
	}

    void OnDestroy()
    {
        UCommandRegister.UnregisterCommand(helpCmd);
    }

    private string HelpCmd(IEnumerable<string> args)
    {
        return "Hello, console!";
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
