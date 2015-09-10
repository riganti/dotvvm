using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Tokenizer
{
    public enum DothtmlTokenType
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
