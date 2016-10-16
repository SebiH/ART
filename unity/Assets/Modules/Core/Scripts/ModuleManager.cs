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
        // we only ever need one instance
        private static ModuleManager _instance;
        private static List<GameObject> _instantiatedModules = new List<GameObject>();

        void OnEnable()
        {
            if (_instance == null)
            {
                // keep object alive for all scene transitions
                DontDestroyOnLoad(this);
                _instance = this;
            }


            foreach (Transform child in transform)
            {
                var module = child.gameObject;
                if (IsModuleLoaded(module))
                {
                    DisableDuplicateModule(module);
                }
                else
                {
                    RegisterModule(module);
                }
            }

            if (_instance != this)
            {
                // script is already active from previous scene; we don't need this one
                Destroy(this);
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

        private void RegisterModule(GameObject module)
        {
            // move object to currently active instance
            if (this != _instance)
            {
                module.transform.parent = _instance.transform;
            }

            _instantiatedModules.Add(module);
        }

        private void DisableDuplicateModule(GameObject module)
        {
            module.SetActive(false);
            Destroy(module);
        }
    }
}
