using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    public class InlineResourceLocation : LocalResourceLocation
    {
        public InlineResourceLocation(string code)
        {
            this.Code = code;
            this.utf8_code = System.Text.Encoding.UTF8.GetBytes(code);
        }
        public string Code { get; }
        private readonly byte[] utf8_code;
        public override Stream LoadResource(IDotvvmRequestContext context) =>
            new MemoryStream(utf8_code, writable: false);
    }
}
