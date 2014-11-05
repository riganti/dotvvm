using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Parser.RwHtml.Tokenizer
{
    public enum RwHtmlTokenType
    {
        OpenTag,
        CloseTag,
        Slash,
        SingleQuote,
        DoubleQuote,
        Equals,
        ExclamationMark,
        QuestionMark,
        DirectiveStart,
        Colon,
        OpenBinding,
        CloseBinding,
        WhiteSpace,
        Text
    }
}
