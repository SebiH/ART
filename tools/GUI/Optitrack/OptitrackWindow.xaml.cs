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

        public OptitrackWindow()
        {
            InitializeComponent();
            _logWindow = new OptitrackLogWindow();
            _logWindow.Show();
            _currentDispatcher = Dispatcher.CurrentDispatcher;
        }


        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            var optitrackIp = OptitrackIpBox.Text;
            var localIp = LocalIpBox.Text;
            var unityIp = UnityIpBox.Text;
            var saveFile = SaveFileBox.Text;
            var loglevel = Int32.Parse(LogLevelBox.Text);

            Task.Run(() =>
            {
                var sb = new StringBuilder();
                DateTime lastUpdate = DateTime.Now;
                DateTime lastClear = DateTime.Now;
                OptitrackServer.OutputCallback callback = (msg) =>
                {
                    sb.Append(msg);

                    // periodically remove complete log
                    if ((DateTime.Now - lastClear).TotalMilliseconds > 2000)
                    {
                        sb = new StringBuilder();
                        lastClear = DateTime.Now;
                    }

                    // reduce amount of calls to current dispatcher
                    if ((DateTime.Now - lastUpdate).TotalMilliseconds > 100)
                    {
                        var output = sb.ToString();
                        lastUpdate = DateTime.Now;
                        _currentDispatcher.InvokeAsync(() =>
                        {
                            _logWindow.Log = output;
                            _logWindow.LogScroller.ScrollToBottom();
                        });
                    }
                };

                OptitrackServer.RegisterCallback(callback);
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
            var loglevel = Int32.Parse(LogLevelBox.Text);

            Task.Run(() =>
            {
                var sb = new StringBuilder();
                DateTime lastUpdate = DateTime.Now;
                OptitrackServer.OutputCallback callback = (msg) =>
                {
                    sb.Append(msg);

                    // reduce amount of calls to current dispatcher
                    if ((DateTime.Now - lastUpdate).TotalMilliseconds > 100)
                    {
                        var output = sb.ToString();
                        lastUpdate = DateTime.Now;
                        _currentDispatcher.InvokeAsync(() =>
                        {
                            _logWindow.Log = output;
                            _logWindow.LogScroller.ScrollToBottom();
                        });
                    }
                };


                OptitrackServer.RegisterCallback(callback);
                OptitrackServer.ReplayFromData(file, loglevel);
            });
        }
    }
}
