using Newtonsoft.Json.Linq;
using System;
using System.Windows;

namespace GUI
{
    public partial class MarkerMapGenerator : Window
    {
        private string _currentToolOutput;
        private void ToolCallback(string json_msg)
        {
            _currentToolOutput = json_msg;
        }

        public MarkerMapGenerator()
        {
            InitializeComponent();
            Loaded += (s, e) => PopulateArucoDictionaries();
        }

        private void PopulateArucoDictionaries()
        {
            ImageProcessing.GetArucoDictionaries(ToolCallback);
            dynamic output = JArray.Parse(_currentToolOutput);

            foreach (var entry in output)
            {
                ArucoDictionaries.Items.Add(((JToken)entry).ToString());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dynamic jsonConfig = new JObject();

                var sizeX = int.Parse(SizeXBox.Text);
                jsonConfig.sizeX = sizeX;
                var sizeY = int.Parse(SizeYBox.Text);
                jsonConfig.sizeY = sizeY;

                var markerIds = new JArray();

                var startId = int.Parse(MarkerIdsBox.Text);
                for (int i = 0; i < sizeX * sizeY; i++)
                {
                    markerIds.Add(startId + i);
                }

                jsonConfig.markerIds = markerIds;
                jsonConfig.imgFilename = ImgFilenameBox.Text;
                jsonConfig.configFilename = ConfigFilenameBox.Text;
                jsonConfig.dictionaryName = ArucoDictionaries.SelectedItem as string;
                jsonConfig.pixelSize = int.Parse(PixelSizeBox.Text);
                jsonConfig.interMarkerDistance = float.Parse(InterMarkerDistanceBox.Text);

                ImageProcessing.GenerateMarkerMap(jsonConfig.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }



        private void AutoGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sets = int.Parse(SetBox.Text);
                var markerIdCounter = int.Parse(MarkerIdsBox.Text);
                for (int setCounter = 0; setCounter < sets; setCounter++)
                {
                    dynamic jsonConfig = new JObject();

                    var sizeX = int.Parse(SizeXBox.Text);
                    jsonConfig.sizeX = sizeX;
                    var sizeY = int.Parse(SizeYBox.Text);
                    jsonConfig.sizeY = sizeY;

                    var markerIds = new JArray();

                    for (int i = 0; i < sizeX * sizeY; i++)
                    {
                        markerIds.Add(markerIdCounter++);
                    }

                    jsonConfig.markerIds = markerIds;
                    jsonConfig.imgFilename = String.Format("map_{0}_{1}.png", ImgFilenameBox.Text, setCounter);
                    jsonConfig.configFilename = String.Format("map_{0}_{1}.yml", ConfigFilenameBox.Text, setCounter);
                    jsonConfig.dictionaryName = ArucoDictionaries.SelectedItem as string;
                    jsonConfig.pixelSize = int.Parse(PixelSizeBox.Text);
                    jsonConfig.interMarkerDistance = float.Parse(InterMarkerDistanceBox.Text);

                    ImageProcessing.GenerateMarkerMap(jsonConfig.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
