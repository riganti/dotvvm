namespace DotVVM.Framework.Parser.Binding.Parser
{
    public class LiteralBindingParserNode : BindingParserNode
    {
        public object Value { get; set; }

        public LiteralBindingParserNode(object value)
        {
            Value = value;
        }
    }
}