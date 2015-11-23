using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

class UpgradeVSProject : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // TODO !


        //Debug.Log("Reimported Asset");

        //string currentDir = Directory.GetCurrentDirectory();
        //string[] slnFile = Directory.GetFiles(currentDir, "*.sln");
        //string[] csprojFile = Directory.GetFiles(currentDir, "*.csproj");

        //if (slnFile.Length > 0 && csprojFile.Length > 0)
        //{
        //    ReplaceInFile(slnFile[0], "Format Version 10.00", "Format Version 11.00");
        //    ReplaceInFile(csprojFile[0], "ToolsVersion=\"3.5\"", "ToolsVersion=\"4.0\"");
        //    ReplaceInFile(csprojFile[0], "<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>", "<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>");
        //    Debug.Log("Upgraded to Visual Studio 2010 Solution");
        //}
    }

    static private void ReplaceInFile(string filePath, string searchText, string replaceText)
    {
        StreamReader reader = new StreamReader(filePath);
        string content = reader.ReadToEnd();
        reader.Close();

        content = Regex.Replace(content, searchText, replaceText);

        StreamWriter writer = new StreamWriter(filePath);
        writer.Write(content);
        writer.Close();
    }
}