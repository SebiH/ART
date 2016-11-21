using UnityEngine;
using System.Collections;

namespace Assets.Modules.Graph
{
    public abstract class DataPoint : MonoBehaviour
    {
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
    }
}
