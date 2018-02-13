using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Properties;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Utils;
using System.Collections.Concurrent;

namespace DotVVM.Compiler.Blazor
{
    public class BlazorCompilingVisitor : ResolvedControlTreeVisitor
    {
        readonly BetterCodeEmitter emitter;
        public BlazorCompilingVisitor(BetterCodeEmitter emitter)
        {
            this.emitter = emitter;
        }

        Dictionary<DataContextStack, ExpressionSyntax> dataContexts = new Dictionary<DataContextStack, ExpressionSyntax>();

        public override void VisitView(ResolvedTreeRoot view)
        {
            this.emitter.PushNewMethod("BuildRenderTree", typeof(void),
                emitter.EmitParameter("builder", typeof(RenderTreeBuilder))
            );
            dataContexts[view.DataContextTypeStack] =
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName("DataContext")
                );
            this.emitter.InitializeDataContext(view.DataContextTypeStack.DataContextType);
            base.VisitView(view);
            this.emitter.PopMethod();
        }

        public ExpressionSyntax CompileBindingExpression(Expression expression)
        {
            var dataContextMap = new ConcurrentDictionary<DataContextStack, ParameterExpression>();
            var replacedExpression = ExpressionUtils.ReplaceAll(expression, p => {
                if (p.GetParameterAnnotation() is BindingParameterAnnotation annotation)
                {
                    if (annotation.ExtensionParameter != null)
                        throw new NotSupportedException();
                    else
                        return dataContextMap.GetOrAdd(annotation.DataContext, _ => Expression.Parameter(_.DataContextType));
                }
                else
                    return p;
            });
            var contextMap2 = dataContextMap.ToDictionary(v => v.Value, v => this.dataContexts[v.Key]);
            return emitter.TranslateExpression(replacedExpression, contextMap2.GetValue);
        }

        public ExpressionSyntax GetPropertyValue(ResolvedPropertySetter setter)
        {
            // TODO: emit two-way binding
            if (setter is ResolvedPropertyValue value)
            {
                return emitter.EmitValue(value.Value);
            }
            else if (setter is ResolvedPropertyBinding binding)
            {
                var function = binding.Binding.Binding.GetProperty<CastedExpressionBindingProperty>(ErrorHandlingMode.ReturnNull);
                return CompileBindingExpression(function.Expression);
            }
            throw new NotSupportedException();
        }

        static ExpressionSyntax ConcatExpressions(IEnumerable<ExpressionSyntax> nodes, string separator) =>
            nodes.Aggregate((a, b) =>
                SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression,
                    SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression,
                        a,
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(separator))
                    ),
                    b
                )
            );

        public static IEnumerable<(string, ExpressionSyntax)> MergeAttributes(IEnumerable<(string name, ExpressionSyntax expr)> attributes) =>
            from a in attributes
            group a by a.name into g
            select (g.Key, ConcatExpressions(g.Select(t => t.expr), HtmlWriter.GetSeparatorForAttribute(g.Key)));

        public void AddAttributesToBuilder(IEnumerable<(string name, ExpressionSyntax expr)> attributes)
        {
            foreach(var (name, value) in MergeAttributes(attributes))
            {
                emitter.EmitAddBuilderAttribute(emitter.EmitValue(name), value);
            }
        }

        void PushDataContext(ExpressionSyntax expr, DataContextStack context)
        {
            var name = emitter.EmitCreateVariable(expr);
            dataContexts[context] = SyntaxFactory.IdentifierName(name);
        }


        static DotvvmPropertyGroup htmlAttributes = DotvvmPropertyGroup.Create(typeof(HtmlGenericControl).GetProperty("Attributes"), null);

        public override void VisitControl(ResolvedControl control)
        {
            if (control.Properties.TryGetValue(DotvvmBindableObject.DataContextProperty, out var newDataContext))
            {
                PushDataContext(GetPropertyValue(newDataContext), control.DataContextTypeStack);
            }
            Action closeElement = () => {};
            if (control.Metadata.Type == typeof(HtmlGenericControl))
            {
                var attributes = new List<(string, ExpressionSyntax)>();
                foreach (var property in control.Properties)
                {
                    var value = this.GetPropertyValue(property.Value);
                    if (property.Key is GroupedDotvvmProperty pg)
                    {
                        if (pg.PropertyGroup == htmlAttributes)
                            attributes.Add((pg.GroupMemberName, value));
                        else if (pg.PropertyGroup == HtmlGenericControl.CssClassesGroupDescriptor)
                            attributes.Add(("class",
                                SyntaxFactory.ConditionalExpression(value,
                                    emitter.EmitValue(pg.GroupMemberName),
                                    emitter.EmitValue("")
                                )
                            ));
                    }
                    else if (property.Key == HtmlGenericControl.VisibleProperty)
                    {
                        attributes.Add(("style", SyntaxFactory.ConditionalExpression(value,
                            emitter.EmitValue("display: none"),
                            emitter.EmitValue("")
                        )));
                    }
                }

                emitter.EmitOpenBuilderElement(emitter.EmitValue((string)control.ConstructorParameters[0]));
                AddAttributesToBuilder(attributes);

                closeElement += () => emitter.EmitCloseBuilderElement();
            }
            else if (control.Metadata.Type == typeof(Framework.Controls.Infrastructure.RawLiteral))
            {
                var text = (string)control.ConstructorParameters[1];
                emitter.EmitAddBuilderText(emitter.EmitValue(text));
            }

            base.VisitControl(control);

            closeElement();
        }
    }
}
