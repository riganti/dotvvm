namespace DotVVM.Framework.Compilation.Parser.Binding.Tokenizer
{
    public enum BindingTokenType
    {
        WhiteSpace,

        Identifier,
        EscapedIdentifier, // has the @prefix
        Dot,
        Comma,

        OpenParenthesis,
        CloseParenthesis,
        OpenArrayBrace,
        CloseArrayBrace,
        OpenCurlyBrace,
        CloseCurlyBrace,

        AddOperator,
        SubtractOperator,
        MultiplyOperator,
        DivideOperator,
        ModulusOperator,
        UnsupportedOperator,

        EqualsEqualsOperator,
        LessThanOperator,
        LessThanEqualsOperator,
        GreaterThanOperator,
        GreaterThanEqualsOperator,
        NotEqualsOperator,

        NotOperator,
        OnesComplementOperator,

        StringLiteralToken,
        InterpolatedStringToken,

        NullCoalescingOperator,
        QuestionMarkOperator,
        ColonOperator,

        AndOperator,
        AndAlsoOperator,
        OrOperator,
        OrElseOperator,
        ExclusiveOrOperator,

        AssignOperator,

        LambdaOperator,

        Semicolon,

        KeywordAbstract,
        KeywordAs,
        KeywordBase,
        KeywordBool,
        KeywordBreak,
        KeywordByte,
        KeywordCase,
        KeywordCatch,
        KeywordChar,
        KeywordChecked,
        KeywordClass,
        KeywordConst,
        KeywordContinue,
        KeywordDecimal,
        KeywordDefault,
        KeywordDelegate,
        KeywordDo,
        KeywordDouble,
        KeywordElse,
        KeywordEnum,
        KeywordEvent,
        KeywordExplicit,
        KeywordExtern,
        KeywordFalse,
        KeywordFinally,
        KeywordFixed,
        KeywordFloat,
        KeywordFor,
        KeywordForeach,
        KeywordGoto,
        KeywordIf,
        KeywordImplicit,
        KeywordIn,
        KeywordInt,
        KeywordInterface,
        KeywordInternal,
        KeywordIs,
        KeywordLock,
        KeywordLong,
        KeywordNamespace,
        KeywordNew,
        KeywordNull,
        KeywordObject,
        KeywordOperator,
        KeywordOut,
        KeywordOverride,
        KeywordParams,
        KeywordPrivate,
        KeywordProtected,
        KeywordPublic,
        KeywordReadonly,
        KeywordRef,
        KeywordReturn,
        KeywordSbyte,
        KeywordSealed,
        KeywordShort,
        KeywordSizeof,
        KeywordStackalloc,
        KeywordStatic,
        KeywordString,
        KeywordStruct,
        KeywordSwitch,
        KeywordThis,
        KeywordThrow,
        KeywordTrue,
        KeywordTry,
        KeywordTypeof,
        KeywordUint,
        KeywordUlong,
        KeywordUnchecked,
        KeywordUnsafe,
        KeywordUshort,
        KeywordUsing,
        KeywordVirtual,
        KeywordVoid,
        KeywordVolatile,
        KeywordWhile
    }


    public static class BindingTokenTypeExtensions
    {
        public static bool IsIdentifier(this BindingTokenType tokenType) =>
            tokenType is BindingTokenType.Identifier or BindingTokenType.EscapedIdentifier;
        public static bool IsIdentifierOrKeyword(this BindingTokenType tokenType) =>
            tokenType is BindingTokenType.Identifier or BindingTokenType.EscapedIdentifier || IsKeyword(tokenType);

        public static bool IsKeyword(this BindingTokenType tokenType) =>
            tokenType >= BindingTokenType.KeywordAbstract && tokenType <= BindingTokenType.KeywordWhile;

        public static bool IsKeywordType(this BindingTokenType tokenType) =>
            tokenType is BindingTokenType.KeywordBool or BindingTokenType.KeywordByte or BindingTokenType.KeywordChar or BindingTokenType.KeywordDecimal or BindingTokenType.KeywordDouble or BindingTokenType.KeywordFloat or BindingTokenType.KeywordInt or BindingTokenType.KeywordLong or BindingTokenType.KeywordObject or BindingTokenType.KeywordSbyte or BindingTokenType.KeywordShort or BindingTokenType.KeywordString or BindingTokenType.KeywordUint or BindingTokenType.KeywordUlong or BindingTokenType.KeywordUshort or BindingTokenType.KeywordVoid;
    }
}
