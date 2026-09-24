using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

static class StylesheetLocator
{
    public static StyleSheet Load([CallerFilePath] string callerPath = "") {
        var folder = Path.GetDirectoryName(callerPath)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(folder)) {
            return null;
        }

        var assetPath = ToProjectRelativePath(folder) + "/styles.uss";
        return AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
    }

    static string ToProjectRelativePath(string absoluteFolder) {
        var dataPath = Application.dataPath.Replace('\\', '/');
        var projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);
        var packagesPath = projectRoot + "Packages";

        if (absoluteFolder.StartsWith(dataPath)) {
            return "Assets" + absoluteFolder.Substring(dataPath.Length);
        }
        if (absoluteFolder.StartsWith(packagesPath)) {
            return "Packages" + absoluteFolder.Substring(packagesPath.Length);
        }

        return absoluteFolder;
    }
}
