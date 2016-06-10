namespace OptitrackManagement
{
    interface IOptitrackSocket
    {
        void Start();
        void Close();
        bool IsInit();
        DataStream GetDataStream();
    }
}
