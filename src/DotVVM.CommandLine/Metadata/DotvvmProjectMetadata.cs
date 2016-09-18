using System;
using System.Collections.Generic;
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


        public string GetUITestProjectFullPath()
        {
            return Path.Combine(ProjectDirectory, UITestProjectPath);
        }
    }
}
