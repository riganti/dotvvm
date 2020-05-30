using System;
using System.IO;

namespace DotVVM.Framework.StartupPerfTests
{
    static internal class FileSystemHelper
    {
        public static string CreateTempDir()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        public static bool RemoveDir(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}