using UnityEngine;

namespace Assets.Modules.Core
{
    public static class Theme
    {
        private static Color[] _themeColors =
        {
            // Material Theme colours - see https://material.io/guidelines/style/color.html
            new Color(244, 67, 54), // red
            new Color(233, 30, 99), // pink
            new Color(156, 39, 176), // purple
            new Color(103, 58, 183), // deep purple
            new Color(63, 81, 181), // indigo
            new Color(33, 150, 243), // blue
            new Color(3, 169, 244), // light blue
            new Color(0, 188, 212), // cyan
            new Color(0, 150, 136), // teal
            new Color(76, 175, 80), // green
            new Color(139, 195, 74), // light green
            new Color(205, 220, 57), // lime
            new Color(255, 235, 59), // yellow
            new Color(255, 193, 7), // amber
            new Color(255, 152, 0), // orange
            new Color(255, 87, 34), // deep orange
            new Color(121, 85, 72), // brown
            new Color(158, 158, 158), // grey
            new Color(96, 125, 139) // blue grey
        };

        public static Color GetColor(int index)
        {
            return _themeColors[index % _themeColors.Length];
        }
    }
}
