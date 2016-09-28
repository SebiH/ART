using System.IO;
using UnityEngine;

namespace Assets.Modules.Core.Code
{
    public static class Paths
    {
        public static string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(Application.dataPath, Path.Combine("../data/", relativePath));
        }
    }
}
