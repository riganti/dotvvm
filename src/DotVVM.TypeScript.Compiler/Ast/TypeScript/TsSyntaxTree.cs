namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsSyntaxTree
    {
        public ISyntaxNode RootNode { get; }

        public TsSyntaxTree(ISyntaxNode rootNode)
        {
            RootNode = rootNode;
        }
    }
}
