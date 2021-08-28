namespace DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer
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
        OpenServerComment,

        OpenDoctype,
        DoctypeBody,
        CloseDoctype,
        
        OpenXmlProcessingInstruction,
        XmlProcessingInstructionBody,
        CloseXmlProcessingInstruction,
        
        DirectiveStart,
        DirectiveName,
        DirectiveValue,
    }
}
