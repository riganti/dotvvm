﻿using System;
using System.IO;
using System.Text;

namespace DotVVM.CommandLine
{
    public static class Names
    {
        public const string ViewModelClassSuffix = "ViewModel";
        public const string ViewsDirectory = "Views";
        public const string ViewModelsDirectory = "ViewModels";

        public static string GetViewModelPath(string viewPath)
        {
            var viewName = Path.GetFileNameWithoutExtension(viewPath);
            var viewLocation = Path.GetDirectoryName(viewPath);
            return Path.Combine(
                GenerateViewModelLocation(viewLocation ?? string.Empty),
                GetViewModel(viewName) + ".cs");
        }

        public static string GetClass(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            var sb = new StringBuilder(name);
            sb[0] = char.ToUpperInvariant(sb[0]);
            return sb.ToString();
        }

        public static string GetViewModel(string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException(nameof(viewName));
            }

            var sb = new StringBuilder(viewName);
            sb[0] = char.ToUpperInvariant(sb[0]);

            if (!viewName.EndsWith(ViewModelClassSuffix, StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(ViewModelClassSuffix);
            }
            return sb.ToString();
        }

        public static string GenerateViewModelLocation(string viewLocation)
        {
            // if the view location contains a folder named Views, change it to ViewModels, otherwise do nothing
            var parts = viewLocation.Split('/', '\\');
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i], ViewsDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    parts[i] = ViewModelsDirectory;
                }
            }
            return string.Join("/", parts);
        }

        public static string GetNamespace(string directory, string projectPath, string rootNamespace)
        {
            var path = GetRelativePath(projectPath, Path.GetFullPath(directory));
            var namespaceName = path.Replace("\\", ".").Replace("/", ".");
            return namespaceName is object
                ? $"{rootNamespace}.{namespaceName}"
                : rootNamespace;
        }

        public static string GetPathFromNamespace(
            string namespaceName,
            string className,
            string rootNamespace,
            string fileExtension,
            string projectDirectory)
        {
            if (!namespaceName.StartsWith(rootNamespace))
            {
                throw new Exception($"The namespace '{namespaceName}' doesn't start with '{rootNamespace}'!");
            }
            var relativeNamespace = namespaceName.Substring(rootNamespace.Length).TrimStart('.');
            var projectRelativePath = Path.Combine(relativeNamespace.Replace('.', '/'), className, fileExtension);
            return Path.Combine(projectDirectory, projectRelativePath);
        }

        public static string GetRelativePath(string relativeTo, string path)
        {
            // TODO: Once .NET Framework is no longer targeted, replace with Path.GetRelativePath
            if (!relativeTo.EndsWith("\\")
                && !relativeTo.EndsWith("/")
                && Directory.Exists(relativeTo))
            {
                relativeTo = $"{relativeTo}{Path.DirectorySeparatorChar}";
            }
            var relativeToUri = new Uri(relativeTo);
            var pathUri = new Uri(path);
            var resultUri = relativeToUri.MakeRelativeUri(pathUri);
            return Uri.UnescapeDataString(resultUri.ToString());
        }
    }
}
