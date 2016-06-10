using System.Net;

namespace OptitrackManagement
{
    interface IOptitrackSocket
    {
        void Start(IPAddress ipAddress, int port);
        void Close();
        bool IsInit();
        DataStream GetDataStream();
    }
}
