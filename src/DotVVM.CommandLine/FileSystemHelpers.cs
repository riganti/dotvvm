using System;
using System.IO;
using System.Text;

namespace DotVVM.CommandLine.Core
{
    public static class FileSystemHelpers
    {
        public static void WriteFile(string fileName, string contents, bool promptOnFileOverwrite = true)
        {
            fileName = Path.GetFullPath(fileName);

            if (File.Exists(fileName) && promptOnFileOverwrite)
            {
                if (!ConsoleHelpers.AskForYesNo($"The file '{fileName}' already exists! Overwrite?"))
                {
                    throw new Exception("The operation was aborted!");
                }
            }

            var directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fileName, contents, Encoding.UTF8);
            Console.WriteLine($"Created {fileName}");
        }
    }
}
