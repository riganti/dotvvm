using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Globalization;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ResourceManagement.ClientGlobalize
{
    public class JQueryGlobalizeResourceLocation : LocalResourceLocation
    {
        private readonly Lazy<byte[]> resultJavascript;
        public JQueryGlobalizeResourceLocation(CultureInfo cultureInfo)
        {
            this.resultJavascript = new Lazy<byte[]>(() => StringUtils.Utf8.GetBytes(JQueryGlobalizeScriptCreator.BuildCultureInfoScript(cultureInfo)));
        }

        public override Stream LoadResource(IDotvvmRequestContext context)
        {
            return new MemoryStream(resultJavascript.Value, writable: false);
        }
    }
}
