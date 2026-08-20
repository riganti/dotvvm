using System;
using System.Collections.Generic;
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
        private const string NoColorOption = "--no-color";
        private const string FilesOption = "--files";

        public CompilerArgs(
            FileInfo assemblyFile,
            DirectoryInfo projectDir,
            bool isHelp = false,
            bool isListProperties = false,
            bool noColor = false,
            IReadOnlyList<string>? filesToCheck = null)
        {
            AssemblyFile = assemblyFile;
            ProjectDir = projectDir;
            IsHelp = isHelp;
            IsListProperties = isListProperties;
            NoColor = noColor;
            FilesToCheck = filesToCheck ?? Array.Empty<string>();
        }

        public FileInfo AssemblyFile { get; init; }
        public DirectoryInfo ProjectDir { get; init; }
        public bool IsHelp { get; init; }
        public bool IsListProperties { get; init;}
        public bool NoColor { get; init; }
        /// <summary>
        /// When non-empty, only diagnostics for these virtual paths are reported.
        /// </summary>
        public IReadOnlyList<string> FilesToCheck { get; init; }

        public static bool TryParse(string[] args, out CompilerArgs parsed)
        {
            // To minimize dependencies, this tool deliberately reinvents the wheel instead of using System.CommandLine.
            parsed = new CompilerArgs(null!, null!);
            int i = 0;
            // First pass: parse options that appear before the positional arguments.
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
                else if (args[i] == NoColorOption)
                {
                    parsed = parsed with { NoColor = true };
                }
                else
                {
                    break;
                }
            }
            // i now contains the number of parsed OPTIONS; expect exactly 2 positional args next.
            if (args.Length - i < 2)
            {
                Console.Error.Write($"The executable expects 2 arguments. Got {args.Length - i}.");
                return false;
            }

            parsed = parsed with {
                AssemblyFile = new FileInfo(args[i]),
                ProjectDir = new DirectoryInfo(args[i + 1])
            };
            i += 2;

            // Second pass: parse options that appear after the positional arguments (e.g. --files).
            while (i < args.Length)
            {
                if (args[i] == FilesOption)
                {
                    ++i;
                    var files = new List<string>();
                    while (i < args.Length && args[i] != FilesOption)
                    {
                        files.Add(args[i]);
                        ++i;
                    }
                    parsed = parsed with { FilesToCheck = files };
                }
                else
                {
                    Console.Error.Write($"Unexpected argument '{args[i]}'.");
                    return false;
                }
            }

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
