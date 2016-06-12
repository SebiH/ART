using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Assets.Code.Util
{
    public class KalmanFilter
    {
        [DllImport("ImageProcessing")]
        private static extern int init(int dynamParams, int measureParams, int controlParams);

        [DllImport("ImageProcessing")]
        private static extern float[] predict(int handle);

        [DllImport("ImageProcessing")]
        private static extern void correct(int handle, float[] measurement, int measurementCount);


        private int _handle = -1;


        public KalmanFilter()
        {

        }

        public KalmanFilter(int dynamParams, int measureParams, int controlParams)
        {
            Init(dynamParams, measureParams, controlParams);
        }

        public void Init(int dynamParams, int measureParams, int controlParams)
        {
            _handle = init(dynamParams, measureParams, controlParams);
        }


        public Vector3 Predict()
        {
            var result = predict(_handle);
            return Vector3.zero;
        }


        public void Correct(float[] measurements)
        {
            correct(_handle, measurements, measurements.Length);
        }
    }
}
