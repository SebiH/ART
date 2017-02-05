using System;
using UnityEngine;
using UnityEngine.VR;

namespace Assets.Modules.Core
{

    // See: https://steamcommunity.com/app/358720/discussions/0/490124466456883617/#c490124466464258141
    public static class VrUtility
    {
        public enum VrMode
        {
            OpenVR, // SteamVR
            Native, // Oculus
            None
        }

        private static VrMode _mode = VrMode.None;
        private static bool _isInitialized = false;

        public static VrMode CurrentMode
        {
            get
            {
                if (!_isInitialized)
                {
                    _mode = VrMode.None;
                    _isInitialized = true;

                    if (!VRSettings.enabled || !VRDevice.isPresent)
                    {
                        //try to enable steamvr then check the result
                        SteamVR.enabled = true;
                        if (SteamVR.enabled)
                        {
                            _mode = VrMode.OpenVR;
                        }
                    }
                    else
                    {
                        SteamVR.enabled = false;
                        _mode = VrMode.Native;
                    }

                    Debug.Log("Setting VR Mode to " + Enum.GetName(typeof(VrMode), _mode));
                }

                return _mode;
            }
        }
    }
}
