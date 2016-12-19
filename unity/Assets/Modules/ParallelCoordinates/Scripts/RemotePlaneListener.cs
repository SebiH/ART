using Assets.Modules.InteractiveSurface;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class RemotePlaneListener : MonoBehaviour
    {
        #region Sync Data Structures

        private struct RemotePlaneCreator
        {
            public int id;
            public string dimX;
            public string dimY;
        }

        private struct RemotePlaneMover
        {
            public int id;
            public float pos;
        }

        private struct RemotePlaneOrder
        {
            public int[] ids;
        }

        #endregion

        private PlaneManager _planeManager;

        void OnEnable()
        {
            _planeManager = GetComponent<PlaneManager>();
            InteractiveSurfaceClient.Instance.OnMessageReceived += OnMessageReceived;
        }

        void OnDisable()
        {
            InteractiveSurfaceClient.Instance.OnMessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(IncomingCommand cmd)
        {
            switch (cmd.command)
            {
                case "plane-order":
                    ReorderPlanes(cmd.payload);
                    break;

                case "plane-position":
                    RepositionPlane(cmd.payload);
                    break;

                case "plane-add":
                    AddPlane(cmd.payload);
                    break;

                case "plane-remove":
                    RemovePlane(int.Parse(cmd.payload));
                    break;

                case "plane-mod":
                    ModifyPlane(cmd.payload);
                    break;
            }
        }

        private void ReorderPlanes(string jsonPayload)
        {
            var payload = JsonUtility.FromJson<RemotePlaneOrder>(jsonPayload);
            _planeManager.SetPlaneOrder(payload.ids);
        }

        private void RepositionPlane(string jsonPayload)
        {
            var payload = JsonUtility.FromJson<RemotePlaneMover>(jsonPayload);
            var plane = _planeManager.Planes.FirstOrDefault(p => p.Id == payload.id);

            if (plane != null)
            {
                plane.SetPosition(DisplayUtility.PixelToUnityCoord(payload.pos));
            }
            else
            {
                Debug.LogWarning("Tried to move nonexistent plane " + payload.id);
            }
        }

        private void AddPlane(string jsonPayload)
        {
            var payload = JsonUtility.FromJson<RemotePlaneCreator>(jsonPayload);
            _planeManager.CreatePlane(payload.id, payload.dimX, payload.dimY);
        }

        private void RemovePlane(int id)
        {
            _planeManager.RemovePlane(id);
        }

        private void ModifyPlane(string jsonPayload)
        {
            // TODO
        }
    }
}
