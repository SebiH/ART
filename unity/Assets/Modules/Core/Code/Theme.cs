using UnityEngine;

namespace Assets.Modules.Core
{
    public static class Theme
    {
        //private static Color[] _themeColors =
        //{
        //    // Material Theme colours - see https://material.io/guidelines/style/color.html
        //    new Color(244f/255f, 67f/255f, 54f/255f), // red
        //    new Color(233f/255f, 30f/255f, 99f/255f), // pink
        //    new Color(156f/255f, 39f/255f, 176f/255f), // purple
        //    new Color(103f/255f, 58f/255f, 183f/255f), // deep purple
        //    new Color(63f/255f, 81f/255f, 181f/255f), // indigo
        //    new Color(33f/255f, 150f/255f, 243f/255f), // blue
        //    new Color(3f/255f, 169f/255f, 244f/255f), // light blue
        //    new Color(0f/255f, 188f/255f, 212f/255f), // cyan
        //    new Color(0f/255f, 150f/255f, 136f/255f), // teal
        //    new Color(76f/255f, 175f/255f, 80f/255f), // green
        //    new Color(139f/255f, 195f/255f, 74f/255f), // light green
        //    new Color(205f/255f, 220f/255f, 57f/255f), // lime
        //    new Color(255f/255f, 235f/255f, 59f/255f), // yellow
        //    new Color(255f/255f, 193f/255f, 7f/255f), // amber
        //    new Color(255f/255f, 152f/255f, 0f/255f), // orange
        //    new Color(255f/255f, 87f/255f, 34f/255f), // deep orange
        //    new Color(121f/255f, 85f/255f, 72f/255f), // brown
        //    new Color(158f/255f, 158f/255f, 158f/255f), // grey
        //    new Color(96f/255f, 125f/255f, 139f/255f) // blue grey
        //};

        private static Color32[] _themeColors32 =
        {
            // indigo
            new Color32(121, 134, 203, 255), // 300
            new Color32(92, 107, 192, 255), // 400
            new Color32(63, 81, 181, 255), // 500
            new Color32(57, 73, 171, 255), // 600
            new Color32(48, 63, 159, 255), // 700

            // blue
            new Color32(100, 181, 246, 255), // 300
            new Color32(66, 165, 245, 255), // 400
            new Color32(33, 150, 243, 255), // 500
            new Color32(30, 136, 229, 255), // 600
            new Color32(25, 118, 210, 255), // 700

            // light blue
            new Color32(79, 195, 247, 255), // 300
            new Color32(41, 182, 246, 255), // 400
            new Color32(3, 169, 244, 255), // 500
            new Color32(3, 155, 229, 255), // 600
            new Color32(2, 136, 209, 255) // 700

        };

        private static Color[] _themeColors =
        {
            //// purples
            //new Color(186/255f, 104f/255f, 200f/255f), // 300
            //new Color(171f/255f, 71f/255f, 188f/255f), // 400 
            //new Color(156f/255f, 39f/255f, 176f/255f), // 500
            //new Color(142f/255f, 36f/255f, 170f/255f), // 600
            //new Color(123f/255f, 31f/255f, 162f/255f), // 700

            //// deep purples
            //new Color(149f/255f, 117f/255f, 205f/255f), // 300
            //new Color(126f/255f, 87f/255f, 194f/255f), // 400
            //new Color(103f/255f, 58f/255f, 183f/255f), // 500
            //new Color(94f/255f, 53f/255f, 177f/255f), // 600
            //new Color(81f/255f, 45f/255f, 168f/255f), // 700

            // indigo
            new Color(121f/255f, 134f/255f, 203f/255f), // 300
            new Color(92f/255f, 107f/255f, 192f/255f), // 400
            new Color(63f/255f, 81f/255f, 181f/255f), // 500
            new Color(57f/255f, 73f/255f, 171f/255f), // 600
            new Color(48f/255f, 63f/255f, 159f/255f), // 700

            // blue
            new Color(100f/255f, 181f/255f, 246f/255f), // 300
            new Color(66f/255f, 165f/255f, 245f/255f), // 400
            new Color(33f/255f, 150f/255f, 243f/255f), // 500
            new Color(30f/255f, 136f/255f, 229f/255f), // 600
            new Color(25f/255f, 118f/255f, 210f/255f), // 700

            // light blue
            new Color(79f/255f, 195f/255f, 247f/255f), // 300
            new Color(41f/255f, 182f/255f, 246f/255f), // 400
            new Color(3f/255f, 169f/255f, 244f/255f), // 500
            new Color(3f/255f, 155f/255f, 229f/255f), // 600
            new Color(2f/255f, 136f/255f, 209f/255f), // 700

        };



        public static Color GetColor(int index)
        {
            return _themeColors[index % _themeColors.Length];
        }

        public static Color32 GetColor32(int index)
        {
            return _themeColors32[index % _themeColors.Length];
        }
    }
}
