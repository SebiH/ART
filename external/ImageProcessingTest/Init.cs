using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using ImageProcessingUtil;
using System.Diagnostics;

namespace ImageProcessingTest
{



    class Init
    {
        static void Main(string[] args)
        {
            using (var capture = new Capture(0))
            {
                capture.SetCaptureProperty(CapProp.FrameWidth, 1280);
                capture.SetCaptureProperty(CapProp.FrameHeight, 720);

                while (true)
                {
                    // in
                    var frame = capture.QueryFrame();
                    var watch = Stopwatch.StartNew();

                    var pose = new MarshalledPose();
                    var resultImg = frame.ToImage<Bgr, byte>();
                    ImageProcessor.TrackMarker(ref resultImg, ref pose);

                    watch.Stop();
                    //Console.Out.WriteLine(String.Format("{0,5:0.0}, {1,5:0.0}, {2,5:0.0}, {3,5:0.0}, {4,5:0.0}, {5,5:0.0}, took {6} ms", pose[0], pose[1], pose[2], pose[3], pose[4], pose[5], watch.ElapsedMilliseconds));

                    CvInvoke.Imshow("bla", resultImg);
                    CvInvoke.WaitKey(30);
                }
            }
        }
    }
}
