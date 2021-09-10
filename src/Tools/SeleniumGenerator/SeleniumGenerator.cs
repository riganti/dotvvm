using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Tools.SeleniumGenerator.Modifications;
namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public abstract class SeleniumGenerator<TControl> : ISeleniumGenerator where TControl : DotvvmBindableObject
    {
        /// <summary>
        /// Gets a list of properties that can be used to determine the control name.
        /// </summary>
        public abstract DotvvmProperty[] NameProperties { get; }

        /// <summary>
        /// Gets a value indicating whether the content of the control can be used to generate the control name.
        /// </summary>
        public abstract bool CanUseControlContentForName { get; }


        public Type ControlType => typeof(TControl);


        /// <summary>
        /// Gets a list of declarations emitted by the control.
        /// </summary>
        public void AddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            string propertyName;

            var htmlName = SelectorStringHelper.TryGetNameFromProperty(context.Control, UITests.NameProperty);
            if (htmlName == null)
            {
                // determine the name
                propertyName = DetermineName(pageObject, context);
            }
            else
            {
                propertyName = htmlName;
            }

            // normalize name
            var normalizedName = SelectorStringHelper.RemoveNonIdentifierCharacters(propertyName);
            // make the name unique
            var uniqueName = MakePropertyNameUnique(context.UsedNames, normalizedName);

            context.UsedNames.Add(uniqueName);
            context.UniqueName = uniqueName;

            // determine the selector
            if (htmlName == null)
            {
                context.Selector = uniqueName;
                AddUITestNameProperty(pageObject, context, uniqueName);
            }
            else
            {
                context.Selector = htmlName;
            }

            context.UsedNames.Add(propertyName);

            AddDeclarationsCore(pageObject, context);
        }

        internal string MakePropertyNameUnique(ICollection<string> usedNames, string selector)
        {
            if (usedNames.Contains(selector))
            {
                var index = 1;
                while (usedNames.Contains(selector + index))
                {
                    index++;
                }

                selector += index;
            }

            return selector;
        }

        public virtual bool CanAddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            return true;
        }

        private void AddUITestNameProperty(PageObjectDefinition pageObject, SeleniumGeneratorContext context, string uniqueName)
        {
            // find end of the tag
            var token = context.Control.DothtmlNode.Tokens.First(t => t.Type == DothtmlTokenType.CloseTag || t.Type == DothtmlTokenType.Slash);
            if (!pageObject.MarkupFileModifications.OfType<UITestNameMarkupFileInsertText>().Any())
            {
                pageObject.MarkupFileModifications.Add(new UITestNameMarkupFileInsertText() {
                    UniqueName = uniqueName,
                    Position = token.StartPosition
                });
            }

        }

        protected virtual string DetermineName(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            // if selector is set, just read it and don't add data context prefixes
            //var shouldAddDataContextPrefixes = uniqueName == null;
            var shouldAddDataContextPrefixes = true;

            // if not found, use the name properties to determine the name

            string uniqueName = null;
            foreach (var nameProperty in NameProperties)
            {
                uniqueName = SelectorStringHelper.TryGetNameFromProperty(context.Control, nameProperty);
                if (uniqueName != null)
                {
                    uniqueName = SelectorStringHelper.NormalizeUniqueName(uniqueName);
                    break;
                }
            }

            // if not found, try to use the content of the control to determine the name
            if (uniqueName == null && CanUseControlContentForName)
            {
                uniqueName = SelectorStringHelper.GetTextFromContent(context.Control.Content);
            }

            // check if control is userControl and assign control's name as unique name
            if (uniqueName == null && context.Control.DothtmlNode is DothtmlElementNode htmlNode)
            {
                uniqueName = htmlNode.TagName;

                // not add DataContext when generating page object for user control
                shouldAddDataContextPrefixes = false;
            }

            // if not found, use control name
            if (uniqueName == null)
            {
                uniqueName = typeof(TControl).Name;
            }

            if (shouldAddDataContextPrefixes)
            {
                uniqueName = SelectorStringHelper.AddDataContextPrefixesToName(pageObject.DataContextPrefixes, uniqueName);
            }

            return uniqueName;
        }


        protected void AddPageObjectProperties(PageObjectDefinition pageObject, SeleniumGeneratorContext context, string type)
        {
            pageObject.Members.Add(GeneratePropertyForProxy(context.UniqueName, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, type));
        }

        protected void AddGenericPageObjectProperties(PageObjectDefinition pageObject,
            SeleniumGeneratorContext context,
            string type,
            string itemHelperName)
        {
            pageObject.Members.Add(GeneratePropertyForProxy(context.UniqueName, type, itemHelperName));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, type, itemHelperName));
        }

        protected void AddControlPageObjectProperty(PageObjectDefinition pageObject, SeleniumGeneratorContext context, string type)
        {
            pageObject.Members.Add(GeneratePropertyForProxy(context.UniqueName, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForControl(context.UniqueName, context.Selector, type));
        }

        protected MemberDeclarationSyntax GeneratePropertyForProxy(string uniqueName, string typeName, params string[] genericTypeNames)
        {
            return SyntaxFactory.PropertyDeclaration(
                    RoslynHelper.ParseTypeName(typeName, genericTypeNames),
                    uniqueName
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                );
        }

        protected StatementSyntax GenerateInitializerForProxy(SeleniumGeneratorContext context, string typeName, params string[] genericTypeNames)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(context.UniqueName),
                    SyntaxFactory.ObjectCreationExpression(RoslynHelper.ParseTypeName(typeName, genericTypeNames))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                SyntaxFactory.Argument(RoslynHelper.GetPathSelectorObjectInitialization(context.Selector))
                            }))
                        )
                )
            );
        }

        protected StatementSyntax GenerateInitializerForControl(string propertyName, string selector, string typeName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeName))
                        .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("webDriver")),
                            SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                            SyntaxFactory.Argument(RoslynHelper.GetPathSelectorObjectInitialization(selector))
                        })))
                )
            );
        }

        protected StatementSyntax GenerateInitializerForTemplate(string propertyName, string typeName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeName))
                        .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("webDriver")),
                            SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("parentSelector"))
                        })))
                )
            );
        }

        protected abstract void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context);

    }
}
