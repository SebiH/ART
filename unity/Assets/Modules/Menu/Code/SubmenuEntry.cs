using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Menu
{
    public abstract class SubmenuEntry : MenuEntry
    {
        private readonly List<MenuEntry> _entries = new List<MenuEntry>();

        protected void AddEntry(MenuEntry entry)
        {
            _entries.Add(entry);
        }

        protected void RemoveEntry(MenuEntry entry)
        {
            _entries.Remove(entry);
        }
    }
}
