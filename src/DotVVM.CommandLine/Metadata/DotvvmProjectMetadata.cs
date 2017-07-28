using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.CommandLine.Metadata
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

    public class ApiClientDefinition
    {
        [JsonProperty("swaggerFile")]
        public Uri SwaggerFile { get; set; }
        [JsonProperty("csharpClient")]
        public string CSharpClient { get; set; }
        [JsonProperty("typescriptClient")]
        public string TypescriptClient { get; set; }
        [JsonProperty("namespace")]
        public string Namespace { get; set; }
        [JsonProperty("compileTypescript")]
        [DefaultValue(true)]
        public bool CompileTypescript { get; set; } = true;
        [JsonProperty("generateWrapperClass")]
        [DefaultValue(true)]
        public bool GenerateWrapperClass { get; set; } = true;
        [DefaultValue(false)]
        [JsonProperty("isSingleClient")]
        public bool IsSingleClient { get; set; }
    }
}
