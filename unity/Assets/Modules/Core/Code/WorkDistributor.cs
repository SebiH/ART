using System;

namespace Assets.Modules.Core
{
    public class WorkDistributor : IDisposable
    {
        public static int GlobalOperations = 0;

        public WorkDistributor()
        {
            GlobalOperations++;
        }

        public bool CanWork()
        {
            if (GlobalOperations > 3)
            {
                // TODO: more elaborate work distribution
                //       e.g. return amount of loops client can perform, allow saving of loops for
                //       operations requiring array resizing etc.
                return UnityEngine.Random.Range(0, GlobalOperations) < 3;
            }

            return true;
        }

        public void Dispose()
        {
            GlobalOperations--;
        }
    }
}
