using System;

namespace Assets.Modules.SurfaceGraphFilters
{
    [Serializable]
    public struct RemoteValueMetadata
    {
        public int id;
        // isFiltered
        public bool f;
        // color in hex, e.g. #FFFFFF
        public string c;
    }
}
