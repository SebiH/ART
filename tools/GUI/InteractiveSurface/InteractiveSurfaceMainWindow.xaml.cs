using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace GUI.InteractiveSurface
{
    /// <summary>
    /// Interaction logic for InteractiveSurfaceMainWindow.xaml
    /// </summary>
    public partial class InteractiveSurfaceMainWindow : Window
    {
        public InteractiveSurfaceMainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                IpLabel.Content = GetLocalIPAddress() + ":81";
            };
        }

        // Taken from http://stackoverflow.com/a/6803109/4090817
        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                ProcessStartInfo pStartInfo = new ProcessStartInfo();
                pStartInfo.WorkingDirectory = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "../../../../scripts/");
                pStartInfo.FileName = "cmd.exe";
                pStartInfo.Arguments = @"/K gui_start_node_server.bat";
                //pStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(pStartInfo);
            });
        }
    }
}
