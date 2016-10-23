using UnityEngine;

namespace Assets.Modules.Menu
{
    public class MenuRenderer : MonoBehaviour
    {
        public Camera AttachedCamera;
        public Vector3 PositionOffset;
        public GameObject ContainerTemplate;

        private GameObject _container;
        private GameObject _root;

        void OnEnable()
        {
            // build up UI
            _root = new GameObject();
            _root.name = "__UiRoot";

            _container = Instantiate(ContainerTemplate);
            _container.name = "__uiPanel";
            _container.transform.SetParent(_root.transform);
            var _containerTransform = _container.transform as RectTransform;

            var currentHeightOffset = _containerTransform.sizeDelta.y/2;

            foreach (Transform child in transform)
            {
                var entry = child.GetComponent<UIElement>();
                var entryObject = entry.CreateElement();
                entryObject.transform.SetParent(_container.transform);
                // container is usually scaled down due to being in world space
                //entryObject.transform.localScale.Scale(_container.transform.localScale);
                entryObject.transform.localScale *= 0.01f;
                // set proper position
                var entryTransform = entryObject.transform as RectTransform;
                var entryHeight = entryTransform.sizeDelta.y;
                entryTransform.localPosition = new Vector3(0, currentHeightOffset - entryHeight / 2f, 0);
                entryTransform.sizeDelta = new Vector2(0, entryHeight);

                currentHeightOffset -= entryHeight;
            }
        }

        void OnDisable()
        {
            Destroy(_container);
        }

        void Update()
        {
            _root.transform.LookAt(AttachedCamera.transform);
            _root.transform.position = AttachedCamera.transform.position + AttachedCamera.transform.rotation * PositionOffset;
            _root.transform.localRotation *= Quaternion.Euler(0, 180, 0);
        }
    }
}
