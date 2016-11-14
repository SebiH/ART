using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public class BarTest : MonoBehaviour
    {
        public string DisplayName = "Surface";

        void Start()
        {
            // random height
            var height = Random.Range(0.1f, 0.7f);
            transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, height / 2, transform.localPosition.z);

            GetComponent<Renderer>().enabled = false;
        }

        void OnTriggerEnter(Collider col)
        {
            Debug.Log("collision");
            if (col.gameObject.name == "SurfaceHitbox")
            {
                GetComponent<Renderer>().enabled = true;
            }
        }

        void OnTriggerExit(Collider col)
        {
            if (col.gameObject.name == "SurfaceHitbox")
            {
                GetComponent<Renderer>().enabled = false;
            }
        }
    }
}
