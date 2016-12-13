using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GUI.Optitrack
{
    public partial class OptitrackWindow : Window
    {
        private Dispatcher _currentDispatcher;
        private bool _isRunning;

        public OptitrackWindow()
        {
            InitializeComponent();
            _currentDispatcher = Dispatcher.CurrentDispatcher;

        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            var optitrackIp = OptitrackIpBox.Text;
            var localIp = LocalIpBox.Text;
            var unityIp = UnityIpBox.Text;
            var saveFile = SaveFileBox.Text;
            var loglevel = Int32.Parse(LogLevelBox.Text);

            var dataPort = 3130;
            var commandPort = 3131;
            var unityPort = 16000;

            Task.Run(() =>
            {
                var sb = new StringBuilder();
                OptitrackServer.LoggerCallback callback = (msg) =>
                {
                    sb.Append(msg);

                    var log = sb.ToString();
                    if (log.Contains("\n"))
                    {
                        log = log.Replace("\n", "");
                        _currentDispatcher.InvokeAsync(() =>
                        {
                            OptitrackLog.Log.Add(log);
                        });

                        sb.Clear();
                    }

                };

                OptitrackServer.SetLogger(loglevel, callback);


                bool success = OptitrackServer.StartOptitrackServer(optitrackIp, dataPort, commandPort, localIp);
                if (success)
                {
                    OptitrackServer.AttachUnityOutput(unityIp, unityPort);

                    // TODO: remove terrible keep-memory-alive hacks
                    _isRunning = true;
                    while (_isRunning)
                    {
                        Thread.Sleep(200); // .. keep memory for log lambda alive
                    }
                }
                else
                {
                    // keep memory alive a bit longer
                    Thread.Sleep(500);
                }
            });
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                OptitrackServer.StopOptitrackServer();
                _isRunning = false;
            });
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            //var file = LoadFileBox.Text;
            //var loglevel = 1;
            MessageBox.Show("NYI");
        }
    }
}
