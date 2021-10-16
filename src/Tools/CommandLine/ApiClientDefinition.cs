using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;

namespace DotVVM.CommandLine
{
    public class ApiClientDefinition
    {
        public const string DefaultSwaggerFile = "openapi.json";
        public const string DefaultCSharpClient = "ApiClient.cs";
        public const string DefaultTypeScriptClient = "ApiClient.ts";
        public const string DefaultInvalidNamespace = "Invalid.Namespace";

        [JsonPropertyName("swaggerFile")]
        public Uri SwaggerFile { get; set; }
            = new Uri(Path.Combine(Directory.GetCurrentDirectory(), DefaultSwaggerFile));

        [JsonPropertyName("csharpClient")]
        public string CSharpClient { get; set; } = DefaultCSharpClient;

        [JsonPropertyName("typescriptClient")]
        public string TypescriptClient { get; set; } = DefaultTypeScriptClient;

        [JsonPropertyName("namespace")]
        public string Namespace { get; set; } = DefaultInvalidNamespace;

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
