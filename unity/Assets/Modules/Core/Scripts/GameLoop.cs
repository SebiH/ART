using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Core
{
    public enum OperationType { Continuous, Batched };
    public enum OperationPriority { Realtime, High, Normal, Low }
    public struct Operation
    {
        public int Id;
        public OperationType Type;
        public int CurrentBatchAmount;
        public OperationPriority Priority;
        public IEnumerator Enumerator;
    }

    /// <summary>
    /// Script for delegating Unity's Update() events to non-script classes to avoid overhead of creating gameObjects
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        public static GameLoop Instance;

        public event Action OnUpdate;
        public event Action OnGameEnd;

        private const int MAX_TICKS_PER_CYCLE = 50000;

        private List<Operation> _operations = new List<Operation>();
        private int _idCounter = 0;
        // class attribute so we can continue loop after next cycle
        private int _loopCounter = 0;

        private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

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
            _stopwatch.Reset();
            _stopwatch.Start();

            if (OnUpdate != null)
            {
                OnUpdate();
            }

            if (_operations.Count > 0)
            {
                var loopStart = _loopCounter % _operations.Count;

                do
                {
                    _loopCounter = _loopCounter % _operations.Count;

                    var operation = _operations[_loopCounter];
                    bool canWork = true;

                    var workDistributor = operation.Enumerator.Current as WaitForAvailableCycles;
                    if (workDistributor != null)
                    {
                        // TODO: dynamic adjustment (based on current workload) + names!
                        workDistributor.Amount -= 200;
                        canWork = workDistributor.Amount <= 0;
                    }

                    if (canWork)
                    {
                        var isFinished = !(operation.Enumerator.MoveNext());
                        if (isFinished) { _operations.RemoveAt(_loopCounter); }
                    }

                    _loopCounter++;
                }
                // iterate at max only one time over list, since many operations are based off Time themselves
                // TODO: could be improved on, to allow additional operations that do not rely on time passing
                while (_stopwatch.ElapsedTicks < MAX_TICKS_PER_CYCLE && loopStart != _loopCounter && _operations.Count > 0);
            }

            _stopwatch.Stop();
        }


        public int StartRoutine(IEnumerator routine, OperationType type = OperationType.Continuous, OperationPriority priority = OperationPriority.Normal)
        {
            var operation = new Operation
            {
                Id = _idCounter,
                Enumerator = routine,
                Priority = priority,
                Type = type
            };
            _idCounter++;
            _operations.Add(operation);

            return operation.Id;
        }

        public void StopRoutine(int operationId)
        {
            //StopCoroutine(routine);
            // TODO.
        }
    }
}
