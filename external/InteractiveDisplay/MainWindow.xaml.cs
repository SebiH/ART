using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace InteractiveDisplay
{
    public partial class MainWindow : Window
    {
        private TcpListener _server;
        private List<Socket> _clients = new List<Socket>();

        public class StateObject
        {
            public const int Size = 1024 * 1024;
            public byte[] Buffer = new byte[Size];
        }


        public MainWindow()
        {
            InitializeComponent();

            _server = new TcpListener(IPAddress.Any, 8835);
            _server.Start();
            bool _isRunning = true;

            Task.Factory.StartNew(() =>
            {
                while (_isRunning)
                {
                    var socket = _server.AcceptSocket();
                    lock (_clients)
                    {
                        _clients.Add(socket);
                    }

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                // we're not expecting any packages
                                socket.Receive(new byte[1]);
                            }
                        }
                        catch (Exception e)
                        {
                            lock (_clients)
                            {
                                _clients.Remove(socket);
                            }
                        }
                    });
                }
            });


            Unloaded += (s, e) =>
            {
                _isRunning = false;
                _server.Stop();
            };
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SendCoordinates(e.GetPosition(this));
        }

        private void Window_TouchMove(object sender, System.Windows.Input.TouchEventArgs e)
        {
            var touchPoint = e.GetTouchPoint(this);
            SendCoordinates(touchPoint.Position);
        }

        private void SendCoordinates(Point coords)
        {
            var rawFloats = new float[] { (float)(coords.X / ActualWidth), (float)(coords.Y / ActualHeight) };
            var rawBytes = new byte[rawFloats.Length * sizeof(float)];
            Buffer.BlockCopy(rawFloats, 0, rawBytes, 0, rawBytes.Length);

            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    try
                    {
                        client.Send(rawBytes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
