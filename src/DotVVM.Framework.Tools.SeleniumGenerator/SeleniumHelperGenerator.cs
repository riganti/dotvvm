using System.IO;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Configuration;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumHelperGenerator
    {

        public ClassDeclarationSyntax ProcessMarkupFile(string filePath, DotvvmConfiguration dotvvmConfiguration)
        {
            // resolve control tree
            var tree = ResolveControlTree(filePath, dotvvmConfiguration);

            // traverse the tree
            var visitor = new SeleniumHelperVisitor();
            visitor.VisitView((ResolvedTreeRoot)tree);

            // return the class
            var name = Path.GetFileNameWithoutExtension(filePath);
            return SyntaxFactory.ClassDeclaration(name).WithMembers(SyntaxFactory.List(visitor.ExportedDeclarations));
        }

        private IAbstractTreeRoot ResolveControlTree(string filePath, DotvvmConfiguration dotvvmConfiguration)
        {
            var fileContent = File.ReadAllText(filePath, Encoding.UTF8);

            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(fileContent);

            var parser = new DothtmlParser();
            var rootNode = parser.Parse(tokenizer.Tokens);

            var treeResolver = new DefaultControlTreeResolver(dotvvmConfiguration);
            return treeResolver.ResolveTree(rootNode, filePath);
        }
    }
}
