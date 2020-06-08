using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace DotVVM.CommandLine.Core.Metadata
{
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

        [JsonProperty("fetchOptions")]
        public ApiClientFetchOptions FetchOptions { get; set; } = new ApiClientFetchOptions();
    }
}
