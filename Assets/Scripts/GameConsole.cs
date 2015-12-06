using Assets.Code.Console;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GameConsole : MonoBehaviour {
    public Text logComponent;
    public InputField commandComponent;
    public GameObject consoleContainer;

    private UConsole console;

    private bool _isActive;
    public bool isActive {
        get
        {
            return _isActive;
        }
        
        set
        {
            _isActive = value;
            onIsActiveChange(value);
        }
    }

    private UConsoleCommand helpCmd;
	void Start ()
    {
        console = new UConsole();
        // make sure we're in the right state
        // TODO: better solution??
        onIsActiveChange(isActive);
	}

    void Update()
    {
        // enable or disable console
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            isActive = !isActive;
            // don't show this key as command in the console
            return;
        }

        if (isActive)
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

    private void onIsActiveChange(bool val)
    {
        consoleContainer.SetActive(val);
    }
	
}
