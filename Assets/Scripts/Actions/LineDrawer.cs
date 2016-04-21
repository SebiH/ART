using Assets.Scripts.GestureControl;
using Assets.Scripts.Gestures;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    private List<Vector3> _positions = new List<Vector3>();
    public Color LineColor = Color.blue;

    public void StartDrawing(GestureEventArgs e)
    {
        _positions.Clear();

        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            _positions.Add(gesture.GetGesturePosition(Hand.Either));
        }
    }

    public void UpdateDrawing(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            _positions.Add(gesture.GetGesturePosition(Hand.Either));
        }
    }

    public void EndDrawing(GestureEventArgs e)
    {
    }

    void OnDrawGizmos()
    {
        Gizmos.color = LineColor;
        for (int i = 1; i < _positions.Count; i++)
        {
            Gizmos.DrawLine(_positions[i - 1], _positions[i]);
        }
    }

}
