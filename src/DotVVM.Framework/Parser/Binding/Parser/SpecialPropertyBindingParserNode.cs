namespace DotVVM.Framework.Parser.Binding.Parser
{
    public class SpecialPropertyBindingParserNode : BindingParserNode
    {
        public BindingSpecialProperty SpecialProperty { get; private set; }

        public SpecialPropertyBindingParserNode(BindingSpecialProperty specialProperty)
        {
            SpecialProperty = specialProperty;
        }
    }
}