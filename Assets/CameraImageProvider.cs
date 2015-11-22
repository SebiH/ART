using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Assets
{
    class CameraImageProvider
    {
        private static bool isRunning;
        private static bool keepRunning = true;

        public static void Init()
        {
            if (!isRunning)
            {
                isRunning = true;
                Thread t = new Thread(new ThreadStart(Run));
                t.Start();
            }

        }

        public static void Stop()
        {
            keepRunning = false;
            isRunning = false;
        }

        private static VectorOfPoint square;
        private static SURF detector;

        private static void Run()
        {
            detector = new SURF(500);

            using (var capture = new Capture(0))
            {
                // find models
                var marker1 = new Image<Gray, byte>("Assets/CV/marker1.png");
                var marker1Keypoints = new VectorOfKeyPoint();
                var marker1Descriptors = new Mat();
                detector.DetectAndCompute(marker1, null, marker1Keypoints, marker1Descriptors, false);

                var marker1DescriptorMatcher = new BFMatcher(DistanceType.L2);
                marker1DescriptorMatcher.Add(marker1Descriptors);

                var marker2 = new Image<Gray, byte>("Assets/CV/marker2.png");
                var marker2Keypoints = new VectorOfKeyPoint();
                var marker2Descriptors = new Mat();
                detector.DetectAndCompute(marker2, null, marker2Keypoints, marker2Descriptors, false);

                var marker2DescriptorMatcher = new BFMatcher(DistanceType.L2);
                marker2DescriptorMatcher.Add(marker2Descriptors);


                square = new VectorOfPoint(
                   new Point[]
                   {
                               new Point(1, 0),
                               new Point(1, 1),
                               new Point(0, 1),
                   });

                while (keepRunning)
                {
                    // in
                    var frame = capture.QueryFrame();
                    var resultImg = new Image<Bgr, byte>(new Size(640, 480));

                    // processing
                    //VectorOfKeyPoint keypoints = new VectorOfKeyPoint();
                    //IOutputArray descriptors = new Mat();
                    //detector.DetectAndCompute(frame, null, keypoints, descriptors, false);

                    //foreach (var keypoint in keypoints.ToArray())
                    //{
                    //    resultImg.Draw(new Rectangle(new Point((int)keypoint.Point.X, (int)keypoint.Point.Y), new Size(1, 1)), new Bgr(0, 0, 255), 3);
                    //}

                    var processingImage = new Mat();

                    CvInvoke.CvtColor(frame, processingImage, ColorConversion.Bgr2Gray);
                    CvInvoke.GaussianBlur(processingImage, processingImage, new Size(5, 5), 1.5, 1.5);

                    CvInvoke.Dilate(processingImage, processingImage, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                    CvInvoke.Erode(processingImage, processingImage, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

                    using (Mat canny = new Mat())
                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                    {
                        CvInvoke.Canny(processingImage, canny, 100, 50);
                        int[,] hierarchy = CvInvoke.FindContourTree(canny, contours, ChainApproxMethod.ChainApproxSimple, new Point());

                        //Image<Gray, byte> tmp = new Image<Gray, byte>(canny.Size);
                        //CvInvoke.DrawContours(tmp, contours, -1, new MCvScalar(255, 255, 255), 1, LineType.EightConnected, null, 2147483647, new Point());
                        //CvInvoke.CvtColor(tmp, resultImg, ColorConversion.Gray2Rgb);
                        //Emgu.CV.UI.ImageViewer.Show(tmp);

                        if (hierarchy.GetLength(0) > 0)
                            FindObjects(frame, contours, hierarchy, 0, marker2DescriptorMatcher);

                        if (stopSignList.Count > 0)
                        {
                            Image<Gray, byte> x = new Image<Gray, byte>(stopSignList[0].Bitmap);
                            CvInvoke.CvtColor(x.Resize(640, 480, Inter.Linear), resultImg, ColorConversion.Gray2Rgb);
                        }
                        else
                            CvInvoke.CvtColor(frame, resultImg, ColorConversion.Bgr2Rgb);
                        //foreach (var rectangle in boxList)
                        //{
                        //    resultImg.Draw(rectangle, new Rgb(0, 0, 255), 2);
                        //}

                        boxList.Clear();
                        stopSignList.Clear();
                    }


                    // out
                    // unity uses RGB byte arrays, and c# methods don't switch channels in byte array!
                    //CvInvoke.CvtColor(frame, resultImg, ColorConversion.Bgr2Rgb);

                    currentImage = resultImg.Bytes;
                    imageGeneration++;
                }
            }

            detector.Dispose();
        }

        private static List<Rectangle> boxList = new List<Rectangle>();
        private static List<Mat> stopSignList = new List<Mat>();

        private static void FindObjects(Mat img, VectorOfVectorOfPoint contours, int[,] hierarchy, int idx, BFMatcher _modelDescriptorMatcher)
        {
            for (; idx >= 0; idx = hierarchy[idx, 0])
            {
                using (VectorOfPoint c = contours[idx])
                using (VectorOfPoint approx = new VectorOfPoint())
                {
                    CvInvoke.ApproxPolyDP(c, approx, CvInvoke.ArcLength(c, true) * 0.02, true);
                    double area = CvInvoke.ContourArea(approx);
                    if (area > 200)
                    {
                        //double ratio = CvInvoke.MatchShapes(square, approx, Emgu.CV.CvEnum.ContoursMatchType.I3);

                        //if (ratio > 0.1) //not a good match of contour shape
                        //{
                        //    //check children
                        //    if (hierarchy[idx, 2] >= 0)
                        //        FindObjects(img, contours, hierarchy, hierarchy[idx, 2], _modelDescriptorMatcher);
                        //    continue;
                        //}

                        Rectangle box = CvInvoke.BoundingRectangle(c);

                        Mat candidate = new Mat();
                        using (Mat tmp = new Mat(img, box))
                            CvInvoke.CvtColor(tmp, candidate, ColorConversion.Bgr2Gray);

                        //set the value of pixels not in the contour region to zero
                        using (Mat mask = new Mat(candidate.Size.Height, candidate.Width, DepthType.Cv8U, 1))
                        {
                            mask.SetTo(new MCvScalar(0));
                            CvInvoke.DrawContours(mask, contours, idx, new MCvScalar(255), -1, LineType.EightConnected, null, int.MaxValue, new Point(-box.X, -box.Y));

                            double mean = CvInvoke.Mean(candidate, mask).V0;
                            CvInvoke.Threshold(candidate, candidate, mean, 255, ThresholdType.Binary);
                            CvInvoke.BitwiseNot(candidate, candidate);
                            CvInvoke.BitwiseNot(mask, mask);

                            candidate.SetTo(new MCvScalar(0), mask);
                        }

                        int minMatchCount = 8;
                        double uniquenessThreshold = 0.8;
                        VectorOfKeyPoint _observeredKeypoint = new VectorOfKeyPoint();
                        Mat _observeredDescriptor = new Mat();
                        detector.DetectAndCompute(candidate, null, _observeredKeypoint, _observeredDescriptor, false);

                        if (_observeredKeypoint.Size >= minMatchCount)
                        {
                            int k = 2;

                            Mat mask;

                            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
                            {
                                _modelDescriptorMatcher.KnnMatch(_observeredDescriptor, matches, k, null);
                                mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                                mask.SetTo(new MCvScalar(255));
                                Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);
                            }

                            int nonZeroCount = CvInvoke.CountNonZero(mask);
                            if (nonZeroCount >= minMatchCount)
                            {
                                boxList.Add(box);
                                stopSignList.Add(candidate);
                            }
                        }
                    }
                }
            }
        }


        private static byte[] currentImage;

        public static byte[] getCurrentImage()
        {
            return currentImage;
        }

        private static int imageGeneration;

        public static int GetImageGeneration()
        {
            return imageGeneration;
        }
    }
}
