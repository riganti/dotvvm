using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class ButtonGenerator : SeleniumGenerator<Button>
    {
        private static readonly DotvvmProperty[] nameProperties = new [] { ButtonBase.TextProperty, ButtonBase.ClickProperty, Validator.ValueProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => true;


        protected override IEnumerable<MemberDeclarationSyntax> GetDeclarationsCore(SeleniumGeneratorContext context)
        {
            var typeName = "DotVVM.Framework.Testing.SeleniumHelpers.ButtonProxy";

            yield return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(typeName),
                context.UniqueName
            )
            .WithInitializer(SyntaxFactory.EqualsValueClause(
                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeName))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new []
                    {
                        SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(context.Selector)))
                    }))
                )
            ));
        }

    }
}
