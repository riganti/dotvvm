using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotVVM.VS2015Extension.Common.LocalEnvironment
{
    public class TemporaryEnvironment
    {
        private string TempFolder = Path.Combine(Path.GetTempPath(), "DotVVM");

        public string GetTempFolder(string sessionFolder)
        {
            return Path.Combine(TempFolder, sessionFolder);
        }

        public void CreateTemporaryFolder(string sessionFolder)
        {
            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);

            if (!Directory.Exists(Path.Combine(TempFolder, sessionFolder)))
                Directory.CreateDirectory(Path.Combine(TempFolder, sessionFolder));
        }

        public void CleanUp(string sessionFolder)
        {
            if (Directory.Exists(GetTempFolder(sessionFolder)))
                Directory.Delete(GetTempFolder(sessionFolder), true);
        }
    }
}