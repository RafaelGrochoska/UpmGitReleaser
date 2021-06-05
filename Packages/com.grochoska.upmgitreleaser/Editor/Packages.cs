using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Editor
{
    [Serializable]
    internal struct PackageInfo
    {
        internal string name;
        internal string version;
    }

    internal enum ReleaseType
    {
        Release,
        Beta,
        Alpha,
        Experimental
    }

    internal static class Packages
    {
        internal static List<string> FetchLocal()
        {
            var packagesDir = Path.Combine(Application.dataPath, "../Packages");

            var validDirectories = Directory.GetDirectories(packagesDir)
                .Select(directory => Path.Combine(directory, "package.json"))
                .Where(File.Exists).ToArray();

            for (int i = 0; i < validDirectories.Count(); i++)
            {
                var splitPath = validDirectories[i].Split('\\', '/');
                validDirectories[i] = splitPath[splitPath.Length - 2];
            }

            return validDirectories.ToList();
        }

        internal static void UpdateVersion(string name, string newVersion)
        {
            var dir = Path.Combine(Application.dataPath, "..", "Packages", name, "package.json");
            var packageInfo = File.ReadAllText(dir);

            var versionRegex = new Regex("\"version\": \"(.*)\"");
            var version = versionRegex.Match(packageInfo).Groups[1].Value;
            packageInfo = packageInfo.Replace(version, newVersion);

            File.WriteAllText(dir, packageInfo);
        }

        internal static PackageInfo RetrieveInfo(string name)
        {
            var dir = Path.Combine(Application.dataPath, "..", "Packages", name, "package.json");
            var packageInfo = File.ReadAllText(dir);

            var versionRegex = new Regex("\"version\": \"(.*)\"");
            var nameRegex = new Regex("\"name\": \"(.*)\"");

            return new PackageInfo
            {
                name = nameRegex.Match(packageInfo).Groups[1].Value,
                version = versionRegex.Match(packageInfo).Groups[1].Value
            };
        }
    }
}