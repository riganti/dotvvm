using System;
using System.IO;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
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
                              + "VS Version: " + DTEHelper.DTE.Version + "\r\n"
                              + "Exception: " + ex
                              + "\r\n\r\n\r\n";
                File.AppendAllText(LogFilePath, message);
            }
            catch
            {
                // ignored
            }
        }
    }
}