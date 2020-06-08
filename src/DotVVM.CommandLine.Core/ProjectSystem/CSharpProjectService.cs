using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DotVVM.CommandLine.Core.ProjectSystem
{
    public class CSharpProjectService
    {
        private XDocument xml;
        private XNamespace ns;
        private string defaultProjectName;

        public void Load(string projectFile)
        {
            xml = XDocument.Load(projectFile);
            ns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

            defaultProjectName = Path.GetFileNameWithoutExtension(projectFile);
        }

        public string GetRootNamespace()
        {
            EnsureProjectLoaded();
            return xml.Root.Descendants(ns + "RootNamespace").FirstOrDefault()?.Value
                ?? xml.Root.Descendants("RootNamespace").FirstOrDefault()?.Value
                ?? defaultProjectName;
        }

        public string GetAssemblyName()
        {
            return xml.Root.Descendants(ns + "AssemblyName").FirstOrDefault()?.Value
                ?? xml.Root.Descendants("AssemblyName").FirstOrDefault()?.Value
                ?? defaultProjectName;
        }


        private void EnsureProjectLoaded()
        {
            if (xml == null)
            {
                throw new InvalidOperationException("The project file is not loaded!");
            }
        }


        public string FindCsprojInDirectory(string directory)
        {
            var projectFiles = Directory.GetFiles(directory, "*.csproj").ToList();
            if (projectFiles.Count > 1)
            {
                throw new Exception("There are multiple *.CSPROJ files in the directory!");
            }
            else if (projectFiles.Count == 1)
            {
                return projectFiles[0];
            }
            else
            {
                return null;
            }
        }

    }
}
