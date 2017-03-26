using UnityEngine;

namespace Assets.Modules.ParallelCoordinates_Deprecated
{
    public class Debug_GraphConnections : MonoBehaviour
    {
        private void LateUpdate()
        {
            for (var i = 0; i < DataLineManager.MaxIndex(); i++)
            {
                var line = DataLineManager.GetLine(i);
                line.IsFiltered = Random.value < 0.75;
                line.Color = Random.ColorHSV();
            }

            enabled = false;
        }
    }
}