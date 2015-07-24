using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;

namespace DotVVM.VS2015Extension
{
    public class LogService
    {

        public static string LogFilePath
        {
            get
            {
                var directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "DotVVM for Visual Studio\\Logs");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return Path.Combine(directory, string.Format("{0:yyyy-MM-dd}.log", DateTime.Now));
            }
        }

        public static void LogError(Exception ex)
        {
            try
            {
                var message = "Date: " + DateTime.Now.ToString("s") + "\r\n"
                              + "OS Version: " + Environment.OSVersion + "\r\n"
                              + "VS Version: " + CompletionHelper.DTE.Version + "\r\n"
                              + "Exception: " + ex
                              + "\r\n\r\n\r\n";
                File.AppendAllText(LogFilePath, message);
            }
            catch
            {
            }
        }

    }
}
