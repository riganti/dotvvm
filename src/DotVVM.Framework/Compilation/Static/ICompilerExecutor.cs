#nullable enable

using System.IO;

namespace DotVVM.Framework.Compilation.Static
{
    public interface ICompilerExecutor
    {
        bool ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir);
    }
}
