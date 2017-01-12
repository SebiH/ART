using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugMenu : MonoBehaviour
{
    public static DebugMenu Instance;

    public GameObject Controller;
    public GameObject Selector;
    public GameObject Tools;

    private List<DebugMenuEntry> _menuEntries = new List<DebugMenuEntry>();
    private DebugMenuEntry _focusedEntry;
    private DebugMenuEntry _selectedEntry;

    public DebugMenu()
        : base()
    {
        Instance = this;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Controller)
        {
            Tools.SetActive(false);
            Selector.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == Controller)
        {
            Tools.SetActive(true);
            Selector.SetActive(false);
        }
    }

    public void AddMenuEntry(DebugMenuEntry entry)
    {
        _menuEntries.Add(entry);
    }

    public void RemoveMenuEntry(DebugMenuEntry entry)
    {
        _menuEntries.Remove(entry);
    }

    public void SelectMenuEntry(GameObject selectedObject)
    {
        UnfocusAll();
        DeselectAll();

        var selectedEntry = selectedObject.GetComponent<DebugMenuEntry>();
        if (selectedEntry != null)
        {
            selectedEntry.Select();
            _selectedEntry = selectedEntry;
        }
    }

    public void DeselectAll()
    {
        if (_selectedEntry != null)
        {
            _selectedEntry.Deselect();
            _selectedEntry = null;
        }
    }

    public void FocusMenuEntry(GameObject focusedObject)
    {
        var focusedEntry = focusedObject.GetComponent<DebugMenuEntry>();
        if (focusedEntry != null)
        {
            focusedEntry.Focus();
            _focusedEntry = focusedEntry;
        }
    }

    public void UnfocusAll()
    {
        if (_focusedEntry != null)
        {
            _focusedEntry.Unfocus();
            _focusedEntry = null;
        }
    }
}
