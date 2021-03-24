using System.IO;

namespace DotVVM.Compiler
{
    public interface ICompilerExecutor
    {
        bool ExecuteCompile(FileInfo assemblyFile, DirectoryInfo? projectDir);
    }
}
