namespace DotVVM.Framework.Parser.Binding.Parser
{
    public class IdentifierNameBindingParserNode : BindingParserNode
    {
        public string Name { get; private set; }

        public IdentifierNameBindingParserNode(string name)
        {
            Name = name;
        }
    }
}