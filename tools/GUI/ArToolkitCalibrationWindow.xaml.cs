using System.IO;
using System.Windows;

namespace GUI
{
    public partial class ArToolkitCalibrationWindow : Window
    {
        public ArToolkitCalibrationWindow()
        {
            InitializeComponent();
        }

        private void StandardCalibrate_Click(object sender, RoutedEventArgs e)
        {
            ImageProcessing.SetOvrCamera(3, 1);
            int cornersX = int.Parse(CornersXBox.Text);
            int cornersY = int.Parse(CornersYBox.Text);
            int calibImages = int.Parse(ImageCountBox.Text);
            double patternWidth = double.Parse(PatternWidthBox.Text);
            double screenSizeMargin = double.Parse(ScreenSizeMarginBox.Text);
            var filename = FilenameBox.Text;
            var currDir = Directory.GetCurrentDirectory();
            var dataDir = Path.Combine(currDir, "../../../../data/");

            ImageProcessing.PerformStandardCalibration(Path.Combine(dataDir, filename), cornersX, cornersY, calibImages, patternWidth, screenSizeMargin);
        }

        private void Calibrate_Click(object sender, RoutedEventArgs e)
        {
            ImageProcessing.SetOvrCamera(3, 0);
            int cornersX = int.Parse(CornersXBox.Text);
            int cornersY = int.Parse(CornersYBox.Text);
            int calibImages = int.Parse(ImageCountBox.Text);
            double patternWidth = double.Parse(PatternWidthBox.Text);
            double screenSizeMargin = double.Parse(ScreenSizeMarginBox.Text);
            var filename = FilenameBox.Text;
            var currDir = Directory.GetCurrentDirectory();
            var dataDir = Path.Combine(currDir, "../../../../data/");

            ImageProcessing.PerformArToolkitCalibration(Path.Combine(dataDir, filename), cornersX, cornersY, calibImages, patternWidth, screenSizeMargin);
        }

        private void StereoCalibrate_Click(object sender, RoutedEventArgs e)
        {
            ImageProcessing.SetOvrCamera(3, 0);
            int cornersX = int.Parse(CornersXBox.Text);
            int cornersY = int.Parse(CornersYBox.Text);
            int calibImages = int.Parse(ImageCountBox.Text);
            double patternWidth = double.Parse(PatternWidthBox.Text);
            double screenSizeMargin = double.Parse(ScreenSizeMarginBox.Text);
            var filename = StereoFileBox.Text;
            var calibLeft = CalibLeftBox.Text;
            var calibRight = CalibRightBox.Text;
            var currDir = Directory.GetCurrentDirectory();
            var dataDir = Path.Combine(currDir, "../../../../data/");

            ImageProcessing.PerformArToolkitStereoCalibration(Path.Combine(dataDir, filename), cornersX, cornersY, calibImages, patternWidth, screenSizeMargin, Path.Combine(dataDir, calibLeft), Path.Combine(dataDir, calibRight));
        }
    }
}
