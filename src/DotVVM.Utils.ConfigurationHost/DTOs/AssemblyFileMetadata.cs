using System;
using System.Collections.Generic;

namespace DotVVM.Compiler
{
    internal class AssemblyFileMetadata
    {
        public Version Version { get; set; }
        public string Location { get; set; }
        public string TargetFramework { get; set; }
        public string FileName { get; set; }
        public List<string> DirectoryFragments { get; set; }
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
    }
}