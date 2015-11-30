using Emgu.CV;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;

namespace ImageProcessingUtil
{
    public static class ImageProcessor
    {
        [DllImport("ImageProcessing")]
        private static extern void DetectMarker([MarshalAs(UnmanagedType.Struct)] ref MarshalledImageData image, [MarshalAs(UnmanagedType.Struct)] ref MarshalledPose pose);

        public static unsafe void TrackMarker(ref Image<Bgr,byte> image, ref MarshalledPose pose)
        {
            // TODO: possible optimisation when handling images, mats in emgucv:
            // http://www.emgu.com/wiki/index.php/Working_with_Images#Creating_Image
            MarshalledImageData unmanagedImage = new MarshalledImageData
            {
                height = image.Height,
                width = image.Width,
                channels = image.NumberOfChannels
            };

            fixed (byte* imgPtr = image.Data)
            {
                unmanagedImage.rawData = imgPtr;
                DetectMarker(ref unmanagedImage, ref pose);
            }
        }
    }
}
