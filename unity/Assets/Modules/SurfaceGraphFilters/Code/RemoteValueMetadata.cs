using System;

namespace Assets.Modules.SurfaceGraphFilters
{
    [Serializable]
    public struct RemoteValueMetadata
    {
        public int id;
        // isFiltered, 1 => true, 0 => false
        public short f;
        // color in hex, e.g. #FFFFFF
        public string c;
    }
}
