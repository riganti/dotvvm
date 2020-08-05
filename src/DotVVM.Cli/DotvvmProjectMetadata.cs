using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace DotVVM.Cli
{
    public class DotvvmProjectMetadata
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("projectName")]
        public string? ProjectName { get; set; }

        [JsonPropertyName("rootNamespace")]
        public string? RootNamespace { get; set; }

        [JsonPropertyName("uiTestProjectPath")]
        public string? UITestProjectPath { get; set; }

        [JsonPropertyName("uiTestProjectRootNamespace")]
        public string? UITestProjectRootNamespace { get; set; }

        [JsonPropertyName("metadataFilePath")]
        public string? MetadataFilePath { get; set; }

        [JsonPropertyName("projectDirectory")]
        public string? ProjectDirectory { get; set; }

        [JsonPropertyName("webAssemblyPath")]
        public string? WebAssemblyPath { get; set; }

        [JsonPropertyName("apiClients")]
        public List<ApiClientDefinition> ApiClients { get; } = new List<ApiClientDefinition>();

        public string? GetUITestProjectFullPath()
        {
            if (ProjectDirectory is null || UITestProjectPath is null)
            {
                return null;
            }
            return Path.Combine(ProjectDirectory, UITestProjectPath);
        }
    }
}
