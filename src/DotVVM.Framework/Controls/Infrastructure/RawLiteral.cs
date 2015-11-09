using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls.Infrastructure
{
    public class RawLiteral: DotvvmControl
    {
        public string EncodedText { get; }
        public bool IsWhitespace { get; }
        public RawLiteral(string text, bool isWhitespace = false)
        {
            EncodedText = text;
            IsWhitespace = isWhitespace;
        }

        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            writer.WriteUnencodedText(EncodedText);
        }
    }
}
