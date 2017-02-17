using System;
using UnityEngine;

namespace Assets.Modules.Core
{
    public class WorkDistributor : IDisposable
    {
        private static int GlobalOperations = 0;
        private static readonly int MAX_WORKLOAD = 300;

        public int AvailableCycles { get; private set; }

        public WorkDistributor()
        {
            GlobalOperations++;
            AvailableCycles = 0;
        }

        // workaround since this is not an attachable script
        public void TriggerUpdate()
        {
            // make sure cycles vary for each object, to avoid framedrop once multiple stashed workers reach threshold
            AvailableCycles += Mathf.FloorToInt((MAX_WORKLOAD / GlobalOperations) * UnityEngine.Random.Range(0.3f, 1.7f));
        }

        public void Deplete(int amount)
        {
            AvailableCycles -= amount;
        }

        public int DepleteAll()
        {
            var cycles = AvailableCycles;
            AvailableCycles = 0;
            return cycles;
        }

        public void Dispose()
        {
            GlobalOperations--;
        }
    }
}
