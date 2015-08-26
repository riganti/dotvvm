namespace DotVVM.Framework.Parser.Binding.Parser
{
    public class LiteralExpressionBindingParserNode : BindingParserNode
    {
        public object Value { get; set; }

        public LiteralExpressionBindingParserNode(object value)
        {
            Value = value;
        }
    }
}