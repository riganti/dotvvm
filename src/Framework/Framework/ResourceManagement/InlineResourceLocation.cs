using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ResourceManagement
{
    public class InlineResourceLocation : LocalResourceLocation
    {
        internal InlineResourceLocation(string? utf16, byte[]? utf8)
        {
            if (utf16 is null && utf8 is null)
                throw new ArgumentException("Either code or utf8 must be provided.");
            this.utf16Cache = utf16;
            this.utf8 = utf8 ?? utf16!.ToUtf8Bytes();

        }
        public InlineResourceLocation(string code) : this(code, null) { }
        public InlineResourceLocation(ImmutableArray<byte> code) : this(null, ImmutableCollectionsMarshal.AsArray(code)) { }
        private readonly byte[] utf8;
        private string? utf16Cache;
        public ImmutableArray<byte> Utf8 => ImmutableCollectionsMarshal.AsImmutableArray(utf8);
        public string Code => utf16Cache ??= StringUtils.Utf8Decode(utf8);
        public override Stream LoadResource(IDotvvmRequestContext context) =>
            new MemoryStream(utf8, writable: false);
    }
}
