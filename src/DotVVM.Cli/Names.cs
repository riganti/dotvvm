using System;
using System.IO;
using System.Text;

namespace DotVVM.Cli
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

        public static string GetViewModel(string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException(nameof(viewName));
            }

            var sb = new StringBuilder(viewName);
            sb[0] = char.ToUpper(sb[0]);

            if (!viewName.EndsWith(ViewModelClassSuffix, StringComparison.CurrentCultureIgnoreCase))
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
                if (String.Equals(parts[i], ViewsDirectory, StringComparison.CurrentCultureIgnoreCase))
                {
                    parts[i] = ViewModelsDirectory;
                }
            }
            return String.Join("/", parts);
        }

        public static string GetNamespace(string directory, string projectPath, string rootNamespace)
        {
            var path = Path.GetRelativePath(projectPath, Path.GetFullPath(directory));
            var namespaceName = path.Replace("\\", ".").Replace("/", ".");
            return namespaceName is object
                ? $"{rootNamespace}.{namespaceName}"
                : rootNamespace;
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
