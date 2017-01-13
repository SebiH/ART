using UnityEngine;

namespace Assets.Modules.Surfaces.Scripts.Manager
{
    public class DummySurface : MonoBehaviour
    {
        public string SurfaceName = "Surface";

        public bool UpdateLocalData = false;
        public int Resolution_Width = 1920;
        public int Resolution_Height = 1080;
        public float PixelToCmRatio = 0.0485f;

        public bool UpdatePositions = true;
        public Transform TopLeftCorner;
        public Transform BottomLeftCorner;
        public Transform BottomRightCorner;
        public Transform TopRightCorner;

        private Surface _createdSurface;

        void OnEnable()
        {
            _createdSurface = SurfaceManager.Instance.Set(SurfaceName,
                TopLeftCorner.position, BottomLeftCorner.position, 
                BottomRightCorner.position, TopRightCorner.position);

            ApplyChanges();
        }

        void Update()
        {
            ApplyChanges();
        }

        private void ApplyChanges()
        {
            if (UpdateLocalData)
            {
                var resolution = new Resolution
                {
                    width = Resolution_Width,
                    height = Resolution_Height
                };
                _createdSurface.DisplayResolution = resolution;
                _createdSurface.PixelToCmRatio = PixelToCmRatio;
            }

            if (UpdatePositions)
            {
                _createdSurface = SurfaceManager.Instance.Set(SurfaceName,
                    TopLeftCorner.position, BottomLeftCorner.position, 
                    BottomRightCorner.position, TopRightCorner.position);
            }

        }
    }
}
