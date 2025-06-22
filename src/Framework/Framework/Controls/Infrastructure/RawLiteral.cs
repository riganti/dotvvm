using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;
using System.Net;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls.Infrastructure
{
    public sealed class RawLiteral : DotvvmControl
    {
        private readonly byte[] _encodedText;
        private readonly byte[] _unencodedText;
        public ReadOnlyMemory<byte> EncodedText => _encodedText;
        public string EncodedTextString => StringUtils.Utf8.GetString(_encodedText);
        public ReadOnlyMemory<byte> UnencodedText => _unencodedText;
        public string UnencodedTextString => StringUtils.Utf8.GetString(_unencodedText);
        public bool IsWhitespace { get; }
        public RawLiteral(byte[] text, byte[] unencodedText, bool isWhitespace = false)
        {
            _encodedText = text;
            _unencodedText = unencodedText;
            IsWhitespace = isWhitespace;
            LifecycleRequirements = ControlLifecycleRequirements.None;
        }

        public static RawLiteral Create(string text)
        {
            var enc = WebUtility.HtmlEncode(text);
            var unencodedBytes = StringUtils.Utf8.GetBytes(text);
            return new RawLiteral(
                object.ReferenceEquals(enc, text) ? unencodedBytes : StringUtils.Utf8.GetBytes(enc),
                unencodedBytes,
                string.IsNullOrWhiteSpace(text));
        }

        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (IsWhitespace)
                writer.WriteUnencodedWhitespace(_encodedText);
            else
                writer.WriteUnencodedText(_encodedText);
        }
    }
}
