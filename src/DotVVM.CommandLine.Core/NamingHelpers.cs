using System;
using System.IO;
using System.Linq;

namespace DotVVM.CommandLine.Core
{
    public static class NamingHelpers
    {
        private const string ViewModelClassNameSuffix = "ViewModel";
        private const string ViewsFolderName = "Views";
        private const string ViewModelsFolderName = "ViewModels";


        public static string GenerateViewModelPath(string viewPath)
        {
            var viewName = GetClassNameFromPath(viewPath);
            var viewLocation = Path.GetDirectoryName(viewPath);
            return Path.Combine(GenerateViewModelLocation(viewLocation), GenerateViewModelName(viewName) + ".cs");
        }

        public static string GenerateViewModelName(string viewName)
        {
            // make first letter upper case
            viewName = MakeFirstLetterUpperCase(viewName);

            // append viewmodel if it is not there
            if (!viewName.EndsWith(ViewModelClassNameSuffix, StringComparison.CurrentCultureIgnoreCase))
            {
                viewName += ViewModelClassNameSuffix;
            }
            else
            {
                viewName = viewName.Substring(0, viewName.Length - ViewModelClassNameSuffix.Length) + ViewModelClassNameSuffix;
            }
            return viewName;
        }

        public static string GenerateCodeBehindClassName(string name)
        {
            // make first letter upper case
            name = MakeFirstLetterUpperCase(name);
            return name;
        }

        private static string MakeFirstLetterUpperCase(string name)
        {
            if (name.Length > 0 && Char.IsLower(name[0]))
            {
                name = name.Substring(0, 1).ToUpper() + name.Substring(1);
            }
            return name;
        }

        public static string GenerateViewModelLocation(string viewLocation)
        {
            // if the view location contains a folder named Views, change it to ViewModels, otherwise do nothing
            var parts = viewLocation.Split('/', '\\');
            for (int i = 0; i < parts.Length; i++)
            {
                if (String.Equals(parts[i], ViewsFolderName, StringComparison.CurrentCultureIgnoreCase))
                {
                    parts[i] = ViewModelsFolderName;
                }
            }
            return String.Join("/", parts);
        }

        public static string GenerateViewModelNamespace(string rootNamespace, string viewModelLocation)
        {
            var parts = new[] { rootNamespace }.Concat(viewModelLocation.Split('/', '\\')).Where(n => !String.IsNullOrEmpty(n));
            return String.Join(".", parts);
        }

        public static string GetClassNameFromPath(string relativePath)
        {
            return Path.GetFileNameWithoutExtension(relativePath);
        }

        public static string GetNamespaceFromPath(string relativePath, string projectPath, string rootNamespace)
        {
            var fullPath = Path.GetFullPath(relativePath);
            var projectRelativePath = PathHelpers.GetRelativePathFrom(projectPath, fullPath);

            var namespaceName = Path.GetDirectoryName(projectRelativePath).Replace("\\", ".").Replace("/", ".");
            return ConcatNamespaces(rootNamespace, namespaceName);
        }

        public static string ConcatNamespaces(string rootNamespace, string namespaceName)
        {
            if (String.IsNullOrEmpty(rootNamespace)) return namespaceName;
            if (String.IsNullOrEmpty(namespaceName)) return rootNamespace;
            return rootNamespace + "." + namespaceName;
        }

        public static string GetPathFromNamespace(string namespaceName, string className, string rootNamespace, string fileExtension, string projectDirectory)
        {
            if (!namespaceName.StartsWith(rootNamespace))
            {
                throw new Exception($"The namespace '{namespaceName}' doesn't start with '{rootNamespace}'!");
            }
            var relativeNamespace = namespaceName.Substring(rootNamespace.Length).TrimStart('.');
            var projectRelativePath = Path.Combine(relativeNamespace.Replace('.', '/'), className, fileExtension);
            return Path.Combine(projectDirectory, projectRelativePath);
        }
    }
}
