using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Core
{
    /// <summary>
    /// Script for delegating Unity's Update() events to non-script classes to avoid overhead of creating gameObjects
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        public static GameLoop Instance;

        public event Action OnUpdate;
        public event Action OnGameEnd;

        private void OnEnable()
        {
            if (Instance) { Debug.LogWarning("Multiple GameLoops not supported!"); }
            Instance = this;
        }

        private void OnDisable()
        {
            if (OnGameEnd != null)
            {
                OnGameEnd();
            }
            Instance = null;
        }

        private void Update()
        {
            if (OnUpdate != null)
            {
                OnUpdate();
            }
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
