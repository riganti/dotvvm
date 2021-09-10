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

        StringLiteralToken,
        InterpolatedStringToken,

        NullCoalescingOperator,
        QuestionMarkOperator,
        ColonOperator,

        AndOperator,
        AndAlsoOperator,
        OrOperator,
        OrElseOperator,

        AssignOperator,

        LambdaOperator,

        Semicolon
    }
}
