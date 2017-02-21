using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Core
{
    public interface DynamicUpdate
    {
        void ManualUpdate();
    }

    /// <summary>
    /// Script for delegating Unity's Update() events to non-script classes to avoid overhead of creating gameObjects
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        public GameLoop Instance;

        private List<DynamicUpdate> _updateRequests = new List<DynamicUpdate>();

        private void OnEnable()
        {
            if (Instance) { Debug.LogWarning("Multiple GameLoops not supported!"); }
            Instance = this;
        }

        private void OnDisable()
        {
            _updateRequests.Clear();
            Instance = null;
        }

        private void Update()
        {
            foreach (var request in _updateRequests)
            {
                request.ManualUpdate();
            }
        }

        public void AddUpdateRequest(DynamicUpdate request)
        {
            _updateRequests.Add(request);
        }

        // Should be used sparingly due to O(n) !
        public void RemoveUpdateRequest(DynamicUpdate request)
        {
            _updateRequests.Remove(request);
        }


        public Coroutine StartRoutine(IEnumerator routine)
        {
            // TODO: implement automatic WorkDistributor?
            return StartCoroutine(routine);
        }

        public void StopRoutine(Coroutine routine)
        {
            StopCoroutine(routine);
        }
    }
}
