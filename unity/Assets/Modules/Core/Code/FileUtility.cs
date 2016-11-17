using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Core
{
    public static class FileUtility
    {
        public static string GetPath(string relativePath)
        {
            return Path.Combine(Application.dataPath, Path.Combine("../data/", relativePath));
        }

        public static void SaveToFile(string filename, string text)
        {
            var path = GetPath(filename);
            File.WriteAllText(path, text);
            Debug.Log(String.Format("Saved to {0}", path));
        }

        public static string LoadFromFile(string filename)
        {
            var path = GetPath(filename);
            Debug.Log(String.Format("Loading from {0}", path));
            var contents = File.ReadAllText(path);
            return contents;
        }
    }
}
