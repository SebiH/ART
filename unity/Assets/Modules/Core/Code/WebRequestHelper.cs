
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Core
{
    public abstract class WebRequestHelper : MonoBehaviour
    {
        public static WebRequestHelper Instance { get; protected set; }

        public abstract void PerformWebRequest(string identifier, WWW request, out WebResult result);


        public class WebResult
        {
            public string text;
        }
    }
}
