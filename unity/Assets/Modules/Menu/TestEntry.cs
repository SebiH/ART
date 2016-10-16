using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Menu
{
    public class TestEntry : MenuEntry
    {
        public string Name;

        public override string GetName()
        {
            return Name;
        }

        public override void OnClicked()
        {

        }
    }
}
