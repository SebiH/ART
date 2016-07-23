using System.Net.Sockets;
using System.Text;

namespace OptitrackManagement
{

    public class DirectStateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 6550700;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

}
