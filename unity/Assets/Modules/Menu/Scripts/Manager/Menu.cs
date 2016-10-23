using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Menu
{
    public class Menu : MonoBehaviour
    {
        public static Menu Instance;

        public MenuRenderer MRenderer;
        public MenuInput MInput;

        private int _selectedEntryIndex = -1;
        private List<UIElement> _uiElements = new List<UIElement>();

        void OnEnable()
        {
            Instance = this;

            MInput.OnButtonPress += OnInput;
            bool hasSelectedEntry = false;
            var entryCounter = 0;

            MRenderer.Initialise();

            foreach (Transform child in transform)
            {
                var entry = child.GetComponent<UIElement>();
                MRenderer.AddElement(entry);
                _uiElements.Add(entry);

                if (!hasSelectedEntry && entry.IsSelectable)
                {
                    SelectEntry(entry);
                    hasSelectedEntry = true;
                    _selectedEntryIndex = entryCounter;
                }

                entryCounter++;
            }
        }


        void OnDisable()
        {

        }

        private void OnInput(InputType input)
        {
            switch (input)
            {
                case InputType.Start:
                    break;
                case InputType.Up:
                    SelectPrevEntry();
                    break;
                case InputType.Down:
                    SelectNextEntry();
                    break;
                case InputType.Confirm:
                    break;
                case InputType.Cancel:
                    break;
            }
        }

        private void SelectEntry(UIElement entry)
        {
            var indexCounter = 0;
            foreach (var el in _uiElements)
            {
                el.SetHighlight(el == entry, Color.green);

                if (el == entry)
                {
                    _selectedEntryIndex = indexCounter;
                }

                indexCounter++;
            }
        }

        private void SelectNextEntry()
        {
            var entryIndex = _selectedEntryIndex + 1;

            while (entryIndex < _uiElements.Count)
            {
                var el = _uiElements[entryIndex];

                if (_uiElements[entryIndex].IsSelectable)
                {
                    SelectEntry(el);
                    break;
                }

                entryIndex++;
            }
        }

        private void SelectPrevEntry()
        {
            var entryIndex = _selectedEntryIndex - 1;

            while (entryIndex >= 0)
            {
                var el = _uiElements[entryIndex];

                if (_uiElements[entryIndex].IsSelectable)
                {
                    SelectEntry(el);
                    break;
                }

                entryIndex--;
            }
        }
    }
}
