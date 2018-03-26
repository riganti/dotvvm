namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsSyntaxTree
    {
        public TsSyntaxNode RootNode { get; }

        public TsSyntaxTree(TsSyntaxNode rootNode)
        {
            RootNode = rootNode;
        }
    }
}