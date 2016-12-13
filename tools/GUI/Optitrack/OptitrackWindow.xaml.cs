using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GUI.Optitrack
{
    public partial class OptitrackWindow : Window
    {
        private OptitrackLogWindow _logWindow;
        private Dispatcher _currentDispatcher;
        private OptitrackServer.LoggerCallback _callback;

        public OptitrackWindow()
        {
            InitializeComponent();
            _logWindow = new OptitrackLogWindow();
            _logWindow.Show();
            _currentDispatcher = Dispatcher.CurrentDispatcher;

            var sb = new StringBuilder();
            OptitrackServer.LoggerCallback _callback = (msg) =>
            {
                sb.Append(msg);

                var log = sb.ToString();
                if (log.Contains(Environment.NewLine))
                {
                    log = log.Replace(Environment.NewLine, "");
                    _currentDispatcher.InvokeAsync(() =>
                    {
                        _logWindow.Log.Add(log);
                        _logWindow.LogScroller.ScrollToBottom();
                    });
                }

            };
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

            //OptitrackServer.SetLogger(loglevel, _callback);
            Task.Run(() =>
            {
                bool success = OptitrackServer.StartOptitrackServer(optitrackIp, dataPort, commandPort, localIp);
                if (success) { OptitrackServer.AttachUnityOutput(unityIp, unityPort); }
            });
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                OptitrackServer.StopOptitrackServer();
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
