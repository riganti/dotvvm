using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DotVVM.CommandLine.Core.Metadata
{
    public class DotvvmProjectMetadata
    {

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        [JsonProperty("rootNamespace")]
        public string RootNamespace { get; set; }

        [JsonProperty("uiTestProjectPath")]
        public string UITestProjectPath { get; set; }

        [JsonProperty("uiTestProjectRootNamespace")]
        public string UITestProjectRootNamespace { get; set; }


        [JsonIgnore()]
        public string MetadataFilePath { get; set; }

        [JsonIgnore()]
        public string ProjectDirectory { get; set; }

        [JsonProperty("apiClients")]
        public List<ApiClientDefinition> ApiClients { get; } = new List<ApiClientDefinition>();


        public string GetUITestProjectFullPath()
        {
            return Path.Combine(ProjectDirectory, UITestProjectPath);
        }
    }
}
