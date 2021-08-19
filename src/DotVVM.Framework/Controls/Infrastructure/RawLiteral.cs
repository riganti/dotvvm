#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;
using System.Net;

namespace DotVVM.Framework.Controls.Infrastructure
{
    public sealed class RawLiteral : DotvvmControl
    {
        public string EncodedText { get; }
        public string UnencodedText { get; }
        public bool IsWhitespace { get; }
        public RawLiteral(string text, string unencodedText, bool isWhitespace = false)
        {
            EncodedText = text;
            UnencodedText = unencodedText;
            IsWhitespace = isWhitespace;
            LifecycleRequirements = ControlLifecycleRequirements.None;
        }

        public static RawLiteral Create(string text) =>
            new RawLiteral(
                WebUtility.HtmlEncode(text),
                text,
                String.IsNullOrWhiteSpace(text));

        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.WriteUnencodedText(EncodedText);
        }
    }
}
