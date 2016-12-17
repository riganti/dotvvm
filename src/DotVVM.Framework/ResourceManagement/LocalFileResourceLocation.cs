using System;
using System.IO;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Represents a resource located in a file in filesystem.
    /// </summary>
    public class LocalFileResourceLocation: LocalResourceLocation, IDebugFileLocalLocation
    {
        public string FilePath { get; }
        public LocalFileResourceLocation(string filePath)
        {
            if (filePath.StartsWith("~/", StringComparison.Ordinal)) filePath = filePath.Substring(2); // trim ~/ from the path
            this.FilePath = filePath;
        }

        public override Stream LoadResource(IDotvvmRequestContext context) => 
            File.OpenRead(Path.Combine(context.Configuration.ApplicationPhysicalPath, FilePath));

        public string GetFilePath(IDotvvmRequestContext context) => FilePath;
    }
}
