using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Menu
{
    public abstract class MenuEntry
    {
        public abstract string GetName();
        public abstract void OnClicked();
    }
}
