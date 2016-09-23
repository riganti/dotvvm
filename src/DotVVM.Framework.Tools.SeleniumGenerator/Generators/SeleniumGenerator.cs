using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Controls;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public abstract class SeleniumGenerator<TControl> : ISeleniumGenerator where TControl : DotvvmControl
    {

        /// <summary>
        /// Gets a list of properties that can be used to determine the control name.
        /// </summary>
        public abstract DotvvmProperty[] NameProperties { get; }

        /// <summary>
        /// Gets a value indicating whether the content of the control can be used to generate the control name.
        /// </summary>
        public abstract bool CanUseControlContentForName { get; }
        

        /// <summary>
        /// Gets a list of declarations emitted by the control.
        /// </summary>
        public void AddDeclarations(HelperDefinition helper, SeleniumGeneratorContext context)
        {
            // determine the name
            var uniqueName = DetermineName(context);

            // make the name unique
            if (context.UsedNames.Contains(uniqueName))
            {
                var index = 1;
                while (context.UsedNames.Contains(uniqueName + index))
                {
                    index++;
                }
                uniqueName = uniqueName + index;
            }
            context.UsedNames.Add(uniqueName);
            context.UniqueName = uniqueName;

            // determine the selector
            var selector = TryGetNameFromProperty(context.Control, UITests.NameProperty);
            if (selector == null)
            {
                selector = uniqueName;

                AddUITestNameProperty(helper, context, uniqueName);
            }
            context.Selector = selector;

            AddDeclarationsCore(helper, context);
        }

        public virtual bool CanAddDeclarations(HelperDefinition helperDefinition, SeleniumGeneratorContext context)
        {
            return true;
        }

        private void AddUITestNameProperty(HelperDefinition helper, SeleniumGeneratorContext context, string uniqueName)
        {
            // find end of the tag
            var token = context.Control.DothtmlNode.Tokens.First(t => t.Type == DothtmlTokenType.CloseTag || t.Type == DothtmlTokenType.Slash);
            
            helper.MarkupFileModifications.Add(new MarkupFileInsertText()
            {
                Text = " UITests.Name=\"" + uniqueName + "\"",
                Position = token.StartPosition
            });
        }


        protected virtual string DetermineName(SeleniumGeneratorContext context)
        {
            // get the name from the UITest.Name property
            var uniqueName = TryGetNameFromProperty(context.Control, UITests.NameProperty);

            // if not found, use the name properties to determine the name
            if (uniqueName == null)
            {
                foreach (var nameProperty in NameProperties)
                {
                    uniqueName = TryGetNameFromProperty(context.Control, nameProperty);
                    if (uniqueName != null)
                    {
                        break;
                    }
                }
            }

            // if not found, try to use the content of the control to determine the name
            if (uniqueName == null && CanUseControlContentForName)
            {
                uniqueName = GetTextFromContent(context.Control.Content);
            }

            // if not found, use control name
            if (uniqueName == null)
            {
                uniqueName = typeof(TControl).Name;
            }
            
            return uniqueName;
        }

        protected string TryGetNameFromProperty(ResolvedControl control, DotvvmProperty property)
        {
            IAbstractPropertySetter setter;
            if (control.TryGetProperty(property, out setter))
            {
                if (setter is ResolvedPropertyValue)
                {
                    return RemoveNonIdentifierCharacters(((ResolvedPropertyValue) setter).Value?.ToString());
                }
                else if (setter is ResolvedPropertyBinding)
                {
                    var binding = ((ResolvedPropertyBinding) setter).Binding;
                    return RemoveNonIdentifierCharacters(binding.Value);
                }
            }
            return null;
        }

        private string RemoveNonIdentifierCharacters(string value)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsLetterOrDigit(value[i]) || value[i] == '_')
                {
                    sb.Append(value[i]);
                }
            }

            if (sb.Length == 0)
            {
                return null;
            }
            else if (char.IsDigit(sb[0]))
            {
                sb.Insert(0, '_');
            }

            return sb.ToString();
        }

        private string GetTextFromContent(IEnumerable<ResolvedControl> controls)
        {
            var sb = new StringBuilder();

            foreach (var control in controls)
            {
                if (control.Metadata.Type == typeof(Literal))
                {
                    sb.Append(TryGetNameFromProperty(control, Literal.TextProperty));
                }
                else if (control.Metadata.Type == typeof(HtmlGenericControl))
                {
                    sb.Append(TryGetNameFromProperty(control, HtmlGenericControl.InnerTextProperty));
                }
            }

            // ensure the text is not too long
            var text = RemoveNonIdentifierCharacters(sb.ToString());
            if (text?.Length > 20)
            {
                text = text.Substring(0, 20);
            }
            return text;
        }


        private static TypeSyntax ParseTypeName(string typeName, params string[] genericTypeNames)
        {
            if (genericTypeNames.Length == 0)
            {
                return SyntaxFactory.ParseTypeName(typeName);
            }
            else
            {
                return SyntaxFactory.GenericName(typeName)
                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                            genericTypeNames.Select(n => SyntaxFactory.ParseTypeName(n)))
                    ));
            }
        }
        

        protected MemberDeclarationSyntax GeneratePropertyForProxy(SeleniumGeneratorContext context, string typeName, params string[] genericTypeNames)
        {
            return SyntaxFactory.PropertyDeclaration(
                    ParseTypeName(typeName, genericTypeNames),
                    context.UniqueName
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                );
        }

        protected StatementSyntax GenerateInitializerForProxy(SeleniumGeneratorContext context, string propertyName, string typeName, params string[] genericTypeNames)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.ObjectCreationExpression(ParseTypeName(typeName, genericTypeNames))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(context.Selector)))
                            }))
                        )
                )
            );
        }

        protected abstract void AddDeclarationsCore(HelperDefinition helper, SeleniumGeneratorContext context);

    }
}