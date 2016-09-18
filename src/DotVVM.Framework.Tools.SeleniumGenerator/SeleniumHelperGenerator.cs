using System.IO;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumHelperGenerator
    {

        public SyntaxTree ProcessMarkupFile(string filePath, DotvvmConfiguration dotvvmConfiguration, SeleniumGeneratorConfiguration seleniumConfiguration)
        {
            // resolve control tree
            var tree = ResolveControlTree(filePath, dotvvmConfiguration);

            // traverse the tree
            var visitor = new SeleniumHelperVisitor();
            visitor.HelperDefinitionsStack.Push(new HelperDefinition() { Name = seleniumConfiguration.HelperName });
            visitor.VisitView((ResolvedTreeRoot)tree);

            // return the class
            return CSharpSyntaxTree.Create(
                SyntaxFactory.CompilationUnit().WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                {
                    SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(seleniumConfiguration.TargetNamespace))
                        .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                        {
                            GenerateClass(visitor.HelperDefinitionsStack.Pop())
                        }))
                }))
                .NormalizeWhitespace()
            );
        }

        private MemberDeclarationSyntax GenerateClass(HelperDefinition helperDefinition)
        {
            return SyntaxFactory.ClassDeclaration(helperDefinition.Name)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(new [] {
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("DotVVM.Framework.Testing.SeleniumHelpers.Proxies.SeleniumHelperBase"))
                 })))
                .WithMembers(SyntaxFactory.List(helperDefinition.Members))
                .AddMembers(
                    SyntaxFactory.ConstructorDeclaration(helperDefinition.Name)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .WithBody(SyntaxFactory.Block(helperDefinition.ConstructorStatements))
                );
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

    public class SeleniumGeneratorConfiguration
    {
        public string TargetNamespace { get; set; }

        public string HelperName { get; set; }
    }
}
