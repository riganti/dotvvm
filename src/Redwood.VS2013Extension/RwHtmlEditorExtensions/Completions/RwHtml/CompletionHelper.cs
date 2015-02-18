using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public static class CompletionHelper
    {
        public static bool IsWhiteSpaceTextToken(RwHtmlToken token)
        {
            return token.Type == RwHtmlTokenType.Text && string.IsNullOrWhiteSpace(token.Text);
        }
    }
}