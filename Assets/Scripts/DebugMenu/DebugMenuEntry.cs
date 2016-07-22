using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DebugMenuEntry : MonoBehaviour
{
    public UnityEvent OnSelected;
    public UnityEvent OnDeselected;
    public UnityEvent OnFocused;
    public UnityEvent OnUnfocused;

    private bool _isSelected = false;
    private bool _isFocused = false;

    void Awake()
    {
        DebugMenu.Instance.AddMenuEntry(this);
    }

    public void Focus()
    {
        if (!_isSelected && !_isFocused)
        {
            OnFocused.Invoke();
        }
    }

    public void Unfocus()
    {
        if (!_isSelected && _isFocused)
        {
            OnUnfocused.Invoke();
        }
    }

    public void Select()
    {
        if (!_isSelected)
        {
            _isSelected = true;
            _isFocused = false;
            OnSelected.Invoke();
        }
    }

    public void Deselect()
    {
        if (_isSelected)
        {
            _isSelected = false;
            OnDeselected.Invoke();
        }
    }
}
