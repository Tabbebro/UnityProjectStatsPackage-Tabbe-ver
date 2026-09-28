using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine.UIElements;

static class StylesheetLocator {
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
        if (absoluteFolder.StartsWith(dataPath)) {
            return "Assets" + absoluteFolder.Substring(dataPath.Length);
        }

        foreach (var package in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages()) {
            var resolvedPath = package.resolvedPath.Replace('\\', '/');
            if (absoluteFolder.StartsWith(resolvedPath)) {
                return package.assetPath + absoluteFolder.Substring(resolvedPath.Length);
            }
        }

        return absoluteFolder;
    }
}