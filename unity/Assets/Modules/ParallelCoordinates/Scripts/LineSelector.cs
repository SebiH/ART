using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(Rigidbody))]
    public class LineSelector : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Trigger Enter");
        }
    }
}
