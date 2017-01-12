using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DebugMenuEntry : MonoBehaviour
{
    public GameObject Visual;

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
            _isFocused = true;
            OnFocused.Invoke();
            Visual.transform.localPosition = Vector3.up * 0.5f;
        }
    }

    public void Unfocus()
    {
        if (!_isSelected && _isFocused)
        {
            _isFocused = false;
            OnUnfocused.Invoke();
            Visual.transform.localPosition = Vector3.zero;
        }
    }

    public void Select()
    {
        if (!_isSelected)
        {
            _isSelected = true;
            _isFocused = false;
            OnSelected.Invoke();
            Visual.transform.localPosition = Vector3.forward * 0.5f;
        }
    }

    public void Deselect()
    {
        if (_isSelected)
        {
            _isSelected = false;
            OnDeselected.Invoke();
            Visual.transform.localPosition = Vector3.zero;
        }
    }
}
