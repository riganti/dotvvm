using System;
using System.IO;
using System.Threading;

namespace DotVVM.Tools.StartupPerfTester
{
    static internal class FileSystemHelper
    {
        public static string CreateTempDir()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        public static void RemoveTempDir(string dir)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                    break;
                }
                catch (IOException)
                {
                    // do nothing when we can't remove the temp directory - some files are locked probably
                }
                catch (UnauthorizedAccessException)
                {
                    // do nothing when we can't remove the temp directory - some files are locked probably
                }
                Thread.Sleep(1000);
            }
        }
    }
}
