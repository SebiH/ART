using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Core.Scripts
{
    /**
     *  Manages persistent instances of singleton scripts, so that they're not 
     *  lost/instantiated twice when switching scenes
     */
    public class ModuleManager : MonoBehaviour
    {
        public GameObject[] Modules;

        private static List<GameObject> _instantiatedModules = new List<GameObject>();

        void OnEnable()
        {
            DontDestroyOnLoad(this);

            foreach (var module in Modules)
            {
                if (!IsModuleLoaded(module))
                {
                    LoadModule(module);
                }
            }
        }

        void OnDisable()
        {
            // ?
        }


        private bool IsModuleLoaded(GameObject module)
        {
            return _instantiatedModules.Any((go) => go.name == module.name);
        }

        private void LoadModule(GameObject module)
        {
            var instance = Instantiate(module);
            instance.transform.parent = transform;
            _instantiatedModules.Add(instance);
        }
    }
}
