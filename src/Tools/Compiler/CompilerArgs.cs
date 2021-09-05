using System;
using System.IO;
using System.Linq;

namespace DotVVM.Compiler
{
    [Serializable]
    public record CompilerArgs
    {
        private static readonly string[] HelpOptions = new string[] {
            "--help", "-h", "-?", "/help", "/h", "/?"
        };
        private const string ListPropertiesOption = "--list-props";

        public CompilerArgs(
            FileInfo assemblyFile,
            DirectoryInfo projectDir,
            bool isHelp = false,
            bool isListProperties = false)
        {
            AssemblyFile = assemblyFile;
            ProjectDir = projectDir;
            IsHelp = isHelp;
            IsListProperties = isListProperties;
        }

        public FileInfo AssemblyFile { get; init; }
        public DirectoryInfo ProjectDir { get; init; }
        public bool IsHelp { get; init; }
        public bool IsListProperties { get; init;}

        public static bool TryParse(string[] args, out CompilerArgs parsed)
        {
            // To minimize dependencies, this tool deliberately reinvents the wheel instead of using System.CommandLine.
            parsed = new CompilerArgs(null!, null!);
            int i = 0;
            for (; i < args.Length; ++i)
            {
                if (HelpOptions.Contains(args[i]))
                {
                    parsed = parsed with { IsHelp = true };
                }
                else if (args[i] == ListPropertiesOption)
                {
                    parsed = parsed with { IsListProperties = true };
                }
                else
                {
                    break;
                }
            }
            // i now contains the number of parsed OPTIONS
            if (args.Length - i != 2)
            {
                Console.Error.Write($"The executable expects 2 arguments. Got {args.Length - i}.");
                return false;
            }

            parsed = parsed with {
                AssemblyFile = new FileInfo(args[i]),
                ProjectDir = new DirectoryInfo(args[i + 1])
            };
            if (!parsed.AssemblyFile.Exists)
            {
                Console.Error.Write($"Assembly '{parsed.AssemblyFile}' does not exist.");
                return false;
            }

            if (!parsed.ProjectDir.Exists)
            {
                Console.Error.Write($"Project directory '{parsed.ProjectDir}' does not exist.");
                return false;
            }
            return true;
        }
    }
}
