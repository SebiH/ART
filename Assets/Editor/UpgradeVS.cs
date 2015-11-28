using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

class UpgradeVSProject : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Debug.Log("Adjusting visual studio files");

        string currentDir = Directory.GetCurrentDirectory();
        string[] slnFiles = Directory.GetFiles(currentDir, "*.sln");
        string[] csprojFiles = Directory.GetFiles(currentDir, "*.csproj");

        //foreach (var slnFile in slnFiles)
        //{
        //    //ReplaceInFile(slnFile, "# Visual Studio 2015", "# Visual Studio 14\r\nVisualStudioVersion = 14.0.23107.0\r\nMinimumVisualStudioVersion = 10.0.40219.1");

        //    // Set to build for x64
        //    ReplaceInFile(slnFile, @"(.*?876C14A4.*?)Any CPU\r\n", @"$1x64\r\n");
        //}

        if (csprojFiles.Length > 0)
        {
            // enable unsafe code blocks - it's enabled in unity via 'gmcs' file, but we also need it in visual studio
            ReplaceInFile(csprojFiles[0], "<AllowUnsafeBlocks>false</AllowUnsafeBlocks>", "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>");
        }
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