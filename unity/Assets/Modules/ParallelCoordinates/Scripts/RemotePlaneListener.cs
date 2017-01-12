using Assets.Modules.Graph;
using Assets.Modules.InteractiveSurface;
using System.Collections;
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

            if (_planeManager.Provider is RemoteDataProvider)
            {
                StartCoroutine(DelayedInitialisation());
            }
            else
            {
                InteractiveSurfaceClient.Instance.OnMessageReceived += OnMessageReceived;
                InteractiveSurfaceClient.Instance.SendCommand(new OutgoingCommand { command = "get-planes" });
                InteractiveSurfaceClient.Instance.SendCommand(new OutgoingCommand { command = "get-plane" });
            }
        }

        private IEnumerator DelayedInitialisation()
        {
            var provider = _planeManager.Provider as RemoteDataProvider;

            while (!provider.IsReady)
            {
                yield return new WaitForSeconds(0.5f);
            }

            InteractiveSurfaceClient.Instance.OnMessageReceived += OnMessageReceived;
            InteractiveSurfaceClient.Instance.SendCommand(new OutgoingCommand { command = "get-planes" });
            InteractiveSurfaceClient.Instance.SendCommand(new OutgoingCommand { command = "get-plane" });
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
            var plane = _planeManager.GetPlane(payload.id);

            if (plane != null)
            {
                // TODO1
                //plane.SetPosition(DisplayUtility.PixelToUnityCoord(payload.pos));
            }
            else
            {
                Debug.LogWarning("Tried to move nonexistent plane " + payload.id);
            }
        }

        private void AddPlane(string jsonPayload)
        {
            var payload = JsonUtility.FromJson<RemotePlaneCreator>(jsonPayload);
            var plane = _planeManager.GetPlane(payload.id);
            plane.SetDimensions(payload.dimX, payload.dimY);
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
