using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DotVVM.Cli
{
    public class ProjectMetadataJson
    {
        [JsonProperty("version")]
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonProperty("packageVersion")]
        [JsonPropertyName("packageVersion")]
        public string? PackageVersion { get; set; }

        [JsonProperty("projectName")]
        [JsonPropertyName("projectName")]
        public string? ProjectName { get; set; }

        [JsonProperty("rootNamespace")]
        [JsonPropertyName("rootNamespace")]
        public string? RootNamespace { get; set; }

        [JsonProperty("uiTestProjectPath")]
        [JsonPropertyName("uiTestProjectPath")]
        public string? UITestProjectPath { get; set; }

        [JsonProperty("uiTestProjectRootNamespace")]
        [JsonPropertyName("uiTestProjectRootNamespace")]
        public string? UITestProjectRootNamespace { get; set; }

        [JsonProperty("metadataFilePath")]
        [JsonPropertyName("metadataFilePath")]
        public string? MetadataFilePath { get; set; }

        [JsonProperty("projectDirectory")]
        [JsonPropertyName("projectDirectory")]
        public string? ProjectDirectory { get; set; }

        [JsonProperty("webAssemblyPath")]
        [JsonPropertyName("webAssemblyPath")]
        public string? WebAssemblyPath { get; set; }

        [JsonProperty("apiClients")]
        [JsonPropertyName("apiClients")]
        public List<ApiClientDefinition>? ApiClients { get; set; }

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
