using DotVVM.Framework.Binding.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DotVVM.Tracing.MiniProfiler.Shared
{
    public class MiniProfilerJavascriptResourceManager
    {
        private static Assembly assembly;
        private static string name;
        static MiniProfilerJavascriptResourceManager()
        {
            assembly = typeof(MiniProfilerJavascriptResourceManager).Assembly;
            name = assembly.GetName().Name;
        }
        public static string GetWigetInlineJavascriptContent()
        {
            using (var ms = new StreamReader(assembly.GetManifestResourceStream($"{name}.MiniProfilerIntegration.js")))
            {
                return ms.ReadToEnd();
            }
        }

    }
}
