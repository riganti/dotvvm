using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotVVM.CommandLine.ProjectSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotVVM.Cli;

namespace DotVVM.CommandLine
{
    public class DotvvmProjectMetadataService
    {

        public const string MetadataFileName = ".dotvvm.json";

        public ProjectMetadataJson FindInDirectory(string directory)
        {
            while (directory != null)
            {
                var fileName = Path.Combine(directory, MetadataFileName);
                if (File.Exists(fileName))
                {
                    return LoadFromFile(fileName);
                }

                directory = Path.GetDirectoryName(directory);
            }
            return null;
        }

        public ProjectMetadataJson LoadFromFile(string file)
        {
            var json = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
            UpgradeToLatestVersion(json);

            var metadata = JsonConvert.DeserializeObject<ProjectMetadataJson>(json.ToString());
            metadata.ProjectDirectory = Path.GetDirectoryName(file);
            metadata.MetadataFilePath = file;
            return metadata;
        }

        public void Save(ProjectMetadataJson metadata)
        {
            File.WriteAllText(metadata.MetadataFilePath, JsonConvert.SerializeObject(metadata, Formatting.Indented));
        }


        private void UpgradeToLatestVersion(JObject json)
        {
            var version = (int)json["version"];

            // place any upgrade code here
        }

        public ProjectMetadataJson CreateDefaultConfiguration(string directory)
        {
            var metadata = new ProjectMetadataJson()
            {
                Version = 1,
                ProjectDirectory = directory,
                MetadataFilePath = Path.Combine(directory, MetadataFileName)
            };

            // find *.csproj file in the directory
            var csprojService = new CSharpProjectService();
            var csproj = csprojService.FindCsprojInDirectory(directory);
            if (csproj != null)
            {
                csprojService.Load(csproj);
                metadata.RootNamespace = csprojService.GetRootNamespace();
                metadata.ProjectName = csprojService.GetAssemblyName();
            }
            return metadata;
        }
    }
}
