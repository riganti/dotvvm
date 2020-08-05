using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DotVVM.Cli
{
    public class ApiClientDefinition
    {
        [JsonPropertyName("swaggerFile")]
        public Uri? SwaggerFile { get; set; }

        [JsonPropertyName("csharpClient")]
        public string? CSharpClient { get; set; }

        [JsonPropertyName("typescriptClient")]
        public string? TypescriptClient { get; set; }

        [JsonPropertyName("namespace")]
        public string? Namespace { get; set; }

        [JsonPropertyName("compileTypescript")]
        [DefaultValue(true)]
        public bool CompileTypescript { get; set; } = true;

        [JsonPropertyName("generateWrapperClass")]
        [DefaultValue(true)]
        public bool GenerateWrapperClass { get; set; } = true;

        [DefaultValue(false)]
        [JsonPropertyName("isSingleClient")]
        public bool IsSingleClient { get; set; }

        [JsonPropertyName("fetchOptions")]
        public ApiClientFetchOptions FetchOptions { get; set; } = new ApiClientFetchOptions();
    }
}
