using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugMenu : MonoBehaviour
{
    public static DebugMenu Instance;

    private List<DebugMenuEntry> _menuEntries = new List<DebugMenuEntry>();
    private DebugMenuEntry _focusedEntry;
    private DebugMenuEntry _selectedEntry;

    public DebugMenu()
        : base()
    {
        Instance = this;
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
