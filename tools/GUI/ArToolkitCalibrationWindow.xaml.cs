using System.Windows;

namespace GUI
{
    public partial class ArToolkitCalibrationWindow : Window
    {
        public ArToolkitCalibrationWindow()
        {
            InitializeComponent();
        }

        private void Calibrate_Click(object sender, RoutedEventArgs e)
        {
            ImageProcessing.SetOvrCamera(2, 0);
            int cornersX = int.Parse(CornersXBox.Text);
            int cornersY = int.Parse(CornersYBox.Text);
            int calibImages = int.Parse(ImageCountBox.Text);
            double patternWidth = double.Parse(PatternWidthBox.Text);
            double screenSizeMargin = double.Parse(ScreenSizeMarginBox.Text);

            ImageProcessing.PerformArToolkitCalibration("test", cornersX, cornersY, calibImages, patternWidth, screenSizeMargin);
        }
    }
}
