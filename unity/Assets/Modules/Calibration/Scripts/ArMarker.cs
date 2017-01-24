using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class ArMarker : MonoBehaviour
    {
        public int Id = -1;
        public Vector2 Position = Vector2.zero;
        public float Size = 0.25f;

        void OnEnable()
        {
            Register();
            ArMarkers.Add(this);
        }

        void OnDisable()
        {
            Deregister();
            ArMarkers.Remove(this);
        }

        void Update()
        {
            transform.localPosition = new Vector3(Position.x, 0, Position.y);
        }

        private void Register()
        {
            // TODO
        }

        private void Deregister()
        {
            // TODO
        }
        
        void OnDrawGizmos()
        {
            // Draw center
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.01f);

            // Draw marker contour
            var tl = transform.position - (transform.rotation * new Vector3(-Size / 2, 0, -Size / 2));
            var tr = transform.position - (transform.rotation * new Vector3(Size / 2, 0, -Size / 2));
            var bl = transform.position - (transform.rotation * new Vector3(-Size / 2, 0, Size / 2));
            var br = transform.position - (transform.rotation * new Vector3(Size / 2, 0, Size / 2));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(tl, bl);
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
        }
    }
}
