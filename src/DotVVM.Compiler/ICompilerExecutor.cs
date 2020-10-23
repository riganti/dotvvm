using System.IO;

namespace DotVVM.Compiler
{
    public interface ICompilerExecutor
    {
        void ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir, string? rootNamespace);
    }
}
