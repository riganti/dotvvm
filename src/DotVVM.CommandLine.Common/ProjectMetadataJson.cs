using System;
using System.Collections.Generic;
using System.IO;

namespace DotVVM.CommandLine
{
    public class ProjectMetadataJson
    {
        public string? AssemblyName { get; set; }

        public string? RootNamespace { get; set; }

        public List<string>? TargetFrameworks { get; set; }

        public string? PackageVersion { get; set; }

        public string? ProjectFilePath { get; set; }

        public string? MetadataFilePath { get; set; }
    }
}
