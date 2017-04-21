using GUI.InteractiveSurface;
using GUI.Optitrack;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    public partial class MainWindow : Window
    {
        private string _currentToolOutput;
        private void ToolCallback(string json_msg)
        {
            _currentToolOutput = json_msg;
        }


        private void JsonMsg(string msg)
        {
            //Debug.WriteLine(msg);
        }

        private void GetPropertiesCallback(string msg)
        {
            //Debug.WriteLine(msg);
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => PopulateArucoDictionaries();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var camera = (CameraSelectionBox.SelectedItem as ComboBoxItem).Content as string;

            Task.Run(() =>
            {
                if (camera == "OVRVision")
                {
                    ImageProcessing.SetOvrCamera(2, 0);
                }
                else if (camera == "OpenCV")
                {
                    ImageProcessing.SetOpenCVCamera();
                }
                else if (camera == "Dummy")
                {
                    ImageProcessing.SetDummyCamera("C:/code/img/4.png");
                }

                ImageProcessing.StartImageProcessing();

                ImageProcessing.GetCamJsonProperties(GetPropertiesCallback);

                int pipeline = ImageProcessing.CreatePipeline();
                int output = ImageProcessing.AddOpenCvOutput(pipeline, "Test");
                int output2 = ImageProcessing.AddJsonOutput(pipeline, JsonMsg);
                //int processor = ImageProcessing.AddArucoProcessor(pipeline, @" { ""marker_size_m"": 0.29, ""use_tracker"": false } ");
                int processor = ImageProcessing.AddArToolkitProcessor(pipeline, @"{ ""config"": { ""calibration_left"": ""C:/code/calib_left.dat"", ""calibration_right"": ""C:/code/calib_right.dat""   }, ""markers"": [  ] }");

                char keyPressed;
                int counter = 0;

                while (true)
                {
                    ImageProcessing.ManualUpdate();
                    keyPressed = (char)ImageProcessing.OpenCvWaitKey(5);

                    if (keyPressed == 's')
                    {
                        counter = (counter + 1) % 2;

                        switch (counter)
                        {
                            case 0:
                                //ImageProcessing.SetOvrCamera(2, 0);
                                ImageProcessing.SetDummyCamera("C:/code/resources/dummy2.jpg");
                                break;
                            case 1:
                                ImageProcessing.SetDummyCamera("C:/code/resources/dummy3.jpg");
                                break;
                        }
                    }

                    if (keyPressed == 'q')
                    {
                        ImageProcessing.RemoveProcessor(pipeline, processor);
                        ImageProcessing.RemoveOutput(pipeline, output);
                        ImageProcessing.RemoveOutput(pipeline, output2);
                        ImageProcessing.RemovePipeline(pipeline);
                        ImageProcessing.StopImageProcessing();
                        break;
                    }
                }
            });
        }

        private void ArToolkitCalibrationClick(object sender, RoutedEventArgs e)
        {
            new ArToolkitCalibrationWindow().Show();
        }

        private void GenerateArucoMarker(object sender, RoutedEventArgs e)
        {
            if (ArucoDictionaries.SelectedIndex == -1)
            {
                MessageBox.Show("Select aruco library first");
                return;
            }

            var dictionary = ArucoDictionaries.SelectedItem as string;
            int markersize = int.Parse(ArucoMarkerSizeBox.Text);
            var saveDir = "markers/";
            Directory.CreateDirectory(saveDir);

            ImageProcessing.GenerateArucoMarkers(dictionary, saveDir, markersize);
        }

        private void PopulateArucoDictionaries()
        {
            //ImageProcessing.GetArucoDictionaries(ToolCallback);
            //dynamic output = JArray.Parse(_currentToolOutput);

            //foreach (var entry in output)
            //{
            //    ArucoDictionaries.Items.Add(((JToken)entry).ToString());
            //}
        }

        private void Optitrack_Click(object sender, RoutedEventArgs e)
        {
            new OptitrackWindow().Show();
        }

        private void Surface_Click(object sender, RoutedEventArgs e)
        {
            new InteractiveSurfaceMainWindow().Show();
        }

        private void GenerateArucoMarkerMapClick(object sender, RoutedEventArgs e)
        {
            new MarkerMapGenerator().Show();
        }
    }
}
