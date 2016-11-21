using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Globalization;

namespace DotVVM.Framework.ResourceManagement.ClientGlobalize
{
    public class JQueryGlobalizeResourceLocation : LocalResourceLocation
    {
        private readonly Lazy<string> resultJavascript;
        public JQueryGlobalizeResourceLocation(CultureInfo cultureInfo)
        {
            this.resultJavascript = new Lazy<string>(() => JQueryGlobalizeScriptCreator.BuildCultureInfoScript(cultureInfo));
        }

        public override Stream LoadResource(IDotvvmRequestContext context)
        {
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(resultJavascript.Value));
        }
    }
}
