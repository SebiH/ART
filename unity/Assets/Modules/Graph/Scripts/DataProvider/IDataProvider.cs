using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Modules.Graph
{
    public abstract class DataProvider : MonoBehaviour
    {
        public abstract float[,] GetData();
    }
}
