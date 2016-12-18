using UnityEngine;

namespace Assets.Modules.Core
{
    public static class Theme
    {
        private static Color[] _themeColors =
        {
            // Material Theme colours - see https://material.io/guidelines/style/color.html
            new Color(244f/255f, 67f/255f, 54f/255f), // red
            new Color(233f/255f, 30f/255f, 99f/255f), // pink
            new Color(156f/255f, 39f/255f, 176f/255f), // purple
            new Color(103f/255f, 58f/255f, 183f/255f), // deep purple
            new Color(63f/255f, 81f/255f, 181f/255f), // indigo
            new Color(33f/255f, 150f/255f, 243f/255f), // blue
            new Color(3f/255f, 169f/255f, 244f/255f), // light blue
            new Color(0f/255f, 188f/255f, 212f/255f), // cyan
            new Color(0f/255f, 150f/255f, 136f/255f), // teal
            new Color(76f/255f, 175f/255f, 80f/255f), // green
            new Color(139f/255f, 195f/255f, 74f/255f), // light green
            new Color(205f/255f, 220f/255f, 57f/255f), // lime
            new Color(255f/255f, 235f/255f, 59f/255f), // yellow
            new Color(255f/255f, 193f/255f, 7f/255f), // amber
            new Color(255f/255f, 152f/255f, 0f/255f), // orange
            new Color(255f/255f, 87f/255f, 34f/255f), // deep orange
            new Color(121f/255f, 85f/255f, 72f/255f), // brown
            new Color(158f/255f, 158f/255f, 158f/255f), // grey
            new Color(96f/255f, 125f/255f, 139f/255f) // blue grey
        };

        public static Color GetColor(int index)
        {
            return _themeColors[index % _themeColors.Length];
        }
    }
}
