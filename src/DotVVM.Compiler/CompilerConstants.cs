namespace DotVVM.Compiler
{
    public class CompilerConstants
    {
        public class EnvironmentVariables
        {
            public const string WebAssemblyPath = "webAssemblyPath";
            public const string AssemblySearchPath = "assemblySearchPath";
            public const string TargetFramework = "targetFramework";
            public const string CompilationConfiguration = "compilationConfiguration";
            
        }
        public class Arguments
        {
            public const string Help = "-?";
            public const string WaitForDebugger = "--debugger";
            public const string WaitForDebuggerAndBreak = "--debugger-break";
            public const string JsonOptions = "--json";
        }
    }
}
