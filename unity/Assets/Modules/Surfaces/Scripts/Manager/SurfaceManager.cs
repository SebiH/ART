using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public class SurfaceManager : MonoBehaviour
    {
        public static SurfaceManager Instance { get; private set; }

        public Surface SurfaceTemplate;

        private readonly Dictionary<string, Surface> _calibratedSurfaces = new Dictionary<string, Surface>();

        void OnEnable()
        {
            Instance = this;
        }

        void OnDisable()
        {

        }

        public bool Has(string name)
        {
            return _calibratedSurfaces.ContainsKey(name);
        }

        public Dictionary<string, Surface> GetAll()
        {
            return _calibratedSurfaces;
        }

        public Surface Get(string name)
        {
            return _calibratedSurfaces[name];
        }

        public Surface Set(string name, Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight)
        {
            Surface surface;

            if (Has(name))
            {
                surface = _calibratedSurfaces[name];
            }
            else
            {
                // set all properties before enabling
                SurfaceTemplate.gameObject.SetActive(false);
                surface = Instantiate(SurfaceTemplate);
                surface.ClientName = name;
                surface.name = name;

                // all properties set - object can be enabled
                surface.gameObject.SetActive(true);

                _calibratedSurfaces.Add(name, surface);
            }

            surface.SetCornerPosition(Corner.TopLeft, topLeft);
            surface.SetCornerPosition(Corner.BottomLeft, bottomLeft);
            surface.SetCornerPosition(Corner.BottomRight, bottomRight);
            surface.SetCornerPosition(Corner.TopRight, topRight);


            return surface;
        }

    }
}
