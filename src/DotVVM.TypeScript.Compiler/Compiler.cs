using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.TypeScript.Compiler
{
    public struct CompilerArguments
    {
        public FileInfo ProjectFile  { get; set; }
    }

    public class Compiler
    {
        private CompilerArguments compilerArguments;

        public Compiler(CompilerArguments compilerArguments)
        {
            this.compilerArguments = compilerArguments;
        }

        public Task RunAsync()
        {
            throw new NotImplementedException();
        }
    }
}
