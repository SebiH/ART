using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace OptitrackManagement
{
    class DirectUnicastSocketClient : IOptitrackSocket
    {
        public void Start(IPAddress ip, int port)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public DataStream GetDataStream()
        {
            throw new NotImplementedException();
        }

        public bool IsInit()
        {
            throw new NotImplementedException();
        }
    }
}
