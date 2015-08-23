namespace DotVVM.Framework.Parser.Binding.Tokenizer
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

        EqualsEqualsOperator,
        LessThanOperator,
        LessThanEqualsOperator,
        GreaterThanOperator,
        GreaterThanEqualsOperator,
        NotEqualsOperator,

        NotOperator,

        StringLiteralToken,

        NullCoalescingOperator,
        QuestionMarkOperator,
        ColonOperator,

        AndOperator,
        AndAlsoOperator,
        OrOperator,
        OrElseOperator
    }
}