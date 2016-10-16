using UnityEngine;

namespace Assets.Modules.Menu
{
    public class Renderer : MonoBehaviour
    {
        public Camera AttachedCamera;
        public Vector3 PositionOffset;

        private GameObject _container;

        void OnEnable()
        {
            // build up UI
            var uiFactory = GetComponent<UIFactory>();
            _container = uiFactory.CreateContainer();

            foreach (Transform child in transform)
            {
                var entry = child.GetComponent<MenuEntry>();
                entry.transform.parent = _container.transform;
            }
        }

        void OnDisable()
        {
            Destroy(_container);
        }

        void Update()
        {
            _container.transform.LookAt(AttachedCamera.transform);
            _container.transform.position = AttachedCamera.transform.position + PositionOffset;
        }
    }
}
