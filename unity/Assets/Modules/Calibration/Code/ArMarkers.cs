using System.Collections.Generic;
using System.Linq;

namespace Assets.Modules.Calibration
{
    public static class ArMarkers
    {
        private static readonly List<ArMarker> _markers = new List<ArMarker>();

        public static void Add(ArMarker marker)
        {
            _markers.Add(marker);
        }

        public static void Remove(ArMarker marker)
        {
            _markers.Remove(marker);
        }

        public static ArMarker Get(int id)
        {
            return _markers.FirstOrDefault(m => m.Id == id);
        }

        public static IEnumerable<ArMarker> GetAll()
        {
            return _markers;
        }
    }
}
