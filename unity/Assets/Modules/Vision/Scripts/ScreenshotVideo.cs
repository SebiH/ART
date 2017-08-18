using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Modules.Vision
{
    public class ScreenshotVideo : MonoBehaviour
    {
        public string Prefix = "cam_";
        private int counter = 0;

        private void Update()
        {
            Application.CaptureScreenshot("D:/GeforceShare/test/" + Prefix + counter + ".jpg", 8);
            counter++;
        }
    }
}
