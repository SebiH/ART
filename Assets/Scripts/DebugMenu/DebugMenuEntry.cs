using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DebugMenuEntry : MonoBehaviour
{
    public UnityEvent OnSelected;
    public UnityEvent OnDeselected;

    private bool _isSelected = false;

    void Awake()
    {
        DebugMenu.Instance.AddMenuEntry(this);
    }

    public void Focus()
    {
        if (!_isSelected)
        {
            GetComponent<Renderer>().material.color = Color.yellow;
        }
    }

    public void Unfocus()
    {
        if (!_isSelected)
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public void Select()
    {
        if (!_isSelected)
        {
            _isSelected = true;
            GetComponent<Renderer>().material.color = Color.red;
            OnSelected.Invoke();
        }
    }

    public void Deselect()
    {
        if (_isSelected)
        {
            _isSelected = false;
            GetComponent<Renderer>().material.color = Color.white;
            OnDeselected.Invoke();
        }
    }
}
