using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Menu
{
    public class MenuToast : MonoBehaviour
    {
        public static MenuToast Instance;

        public GameObject TextPrefab;
        public Camera AttachedCamera;
        public Vector3 Offset;

        void OnEnable()
        {
            Instance = this;
        }

        void OnDisable()
        {
        }

        public void Info(string msg)
        {
            var go = Instantiate(TextPrefab);
            go.transform.position = AttachedCamera.transform.position + Offset;
            var textContainer = go.transform.GetChild(0).GetComponent<Text>();
            textContainer.text = msg;

            StartCoroutine(FadeOut(go, 2f));
        }

        private IEnumerator FadeOut(GameObject go, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Destroy(go);
        }
    }
}
