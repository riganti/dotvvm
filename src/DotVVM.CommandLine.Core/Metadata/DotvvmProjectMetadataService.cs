using System.IO;
using System.Text;
using DotVVM.CommandLine.Core.ProjectSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.CommandLine.Core.Metadata
{
    public class DotvvmProjectMetadataService
    {

        public const string MetadataFileName = ".dotvvm.json";

        public DotvvmProjectMetadata FindInDirectory(string directory)
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

        public DotvvmProjectMetadata LoadFromFile(string file)
        {
            var json = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
            UpgradeToLatestVersion(json);

            var metadata = JsonConvert.DeserializeObject<DotvvmProjectMetadata>(json.ToString());
            metadata.ProjectDirectory = Path.GetDirectoryName(file);
            metadata.MetadataFilePath = file;
            return metadata;
        }

        public void Save(DotvvmProjectMetadata metadata)
        {
            File.WriteAllText(metadata.MetadataFilePath, JsonConvert.SerializeObject(metadata, Formatting.Indented));
        }


        private void UpgradeToLatestVersion(JObject json)
        {
            var version = (int)json["version"];

            // place any upgrade code here
        }

        public DotvvmProjectMetadata CreateDefaultConfiguration(string directory)
        {
            var metadata = new DotvvmProjectMetadata()
            {
                Version = 2,
                ProjectDirectory = directory,
                MetadataFilePath = Path.Combine(directory, MetadataFileName),
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
