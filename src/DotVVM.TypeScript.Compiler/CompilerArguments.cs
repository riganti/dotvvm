using System.IO;

namespace DotVVM.TypeScript.Compiler
{
    public struct CompilerArguments
    {
        public FileInfo SolutionFile { get; set; }
        public string ProjectName { get; set; }
    }
}