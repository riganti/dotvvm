namespace DotVVM.Framework.Compilation.Parser.Binding.Tokenizer
{
    public enum BindingTokenType
    {
        WhiteSpace,

        Identifier,
        Dot,
        Comma,

        OpenParenthesis,
        CloseParenthesis,
        OpenArrayBrace,
        CloseArrayBrace,

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

        Semicolon
    }
}
