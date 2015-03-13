using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Parser.RwHtml.Tokenizer
{
    public enum RwHtmlTokenType
    {
        WhiteSpace,
        Text,

        OpenTag,
        CloseTag,
        Colon,
        Slash,
        SingleQuote,
        DoubleQuote,
        Equals,
        ExclamationMark,
        QuestionMark,
        
        OpenBinding,
        CloseBinding,

        OpenCData,
        CDataBody,
        CloseCData,

        OpenComment,
        CommentBody,
        CloseComment,

        OpenDoctype,
        DoctypeBody,
        CloseDoctype,
        
        OpenXmlProcessingInstruction,
        XmlProcessingInstructionBody,
        CloseXmlProcessingInstruction,
        
        DirectiveStart,
        DirectiveName,
        DirectiveValue
    }
}
