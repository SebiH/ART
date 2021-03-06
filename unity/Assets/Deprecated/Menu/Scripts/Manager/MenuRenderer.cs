using UnityEngine;

namespace Assets.Modules.Menu
{
    public class MenuRenderer : MonoBehaviour
    {
        public Camera AttachedCamera;
        public GameObject ContainerTemplate;
        public Vector3 PositionOffset;

        private GameObject _container;
        private GameObject _root;
        private float _heightOffset;

        void OnEnable()
        {
        }

        void OnDisable()
        {
            Destroy(_root);
        }

        void Update()
        {
            _root.transform.localPosition = AttachedCamera.transform.position + AttachedCamera.transform.rotation * PositionOffset;
            _root.transform.LookAt(AttachedCamera.transform);
            _root.transform.localRotation *= Quaternion.Euler(0, 180, 0);
        }

        public void Initialise()
        {
            // build up UI
            _root = new GameObject();
            _root.name = "__UiRoot";
            // start off as inactive
            _root.SetActive(false);

            _container = Instantiate(ContainerTemplate);
            _container.name = "__uiPanel";
            _container.transform.SetParent(_root.transform);
            var _containerTransform = _container.transform as RectTransform;

            _heightOffset = _containerTransform.sizeDelta.y / 2;
        }


        public void AddElement(UIElement element)
        {
            var entryObject = element.CreateElement();
            entryObject.transform.SetParent(_container.transform);
            // container is usually scaled down due to being in world space
            //entryObject.transform.localScale.Scale(_container.transform.localScale);
            entryObject.transform.localScale *= 0.01f;
            // set proper position
            var entryTransform = entryObject.transform as RectTransform;
            var entryHeight = entryTransform.sizeDelta.y;
            entryTransform.localPosition = new Vector3(0, _heightOffset - entryHeight / 2f, 0);
            entryTransform.sizeDelta = new Vector2(0, entryHeight);

            _heightOffset -= entryHeight;
        }

        public void ToggleVisibility()
        {
            _root.SetActive(!_root.activeSelf);
        }
    }
}
