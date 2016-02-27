using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Gesture
{
    public abstract class GestureBase
    {
        public abstract string GetName();

        public abstract bool CheckConditions();
    }
}
