using System;
using System.Threading.Tasks;
using System.Windows;

namespace GUI.Optitrack
{
    public partial class OptitrackWindow : Window
    {
        private bool _isServerRunning = false;

        public OptitrackWindow()
        {
            InitializeComponent();
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            _isServerRunning = true;

            var optitrackIp = OptitrackIpBox.Text;
            var localIp = LocalIpBox.Text;
            var unityIp = UnityIpBox.Text;
            var saveFile = SaveFileBox.Text;
            var loglevel = Int32.Parse(LogLevelBox.Text);

            Task.Run(() =>
            {
                OptitrackServer.StartServer(optitrackIp, localIp, unityIp, saveFile, loglevel);
            });
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                OptitrackServer.StopServer();
            });
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            var file = LoadFileBox.Text;

            Task.Run(() =>
            {
                OptitrackServer.ReplayFromData(file);
            });
        }
    }
}
