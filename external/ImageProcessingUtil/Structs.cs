using System.Runtime.InteropServices;

namespace ImageProcessingUtil
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public unsafe struct MarshalledImageData
    {
        /// <summary>
        /// Pointer to the byte array containing the image (*has* to be of length width x height x type)
        /// </summary>
        public byte* rawData;

        /// <summary>
        /// Width of the image
        /// </summary>
        public int width;

        /// <summary>
        /// Height of the image
        /// </summary>
        public int height;

        /// <summary>
        /// Number of channels in the image
        /// </summary>
        public int channels;
    };


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public unsafe struct MarshalledPose
    {
        public double translationX;
        public double translationY;
        public double translationZ;

        public double rotationX;
        public double rotationY;
        public double rotationZ;
    };


}
