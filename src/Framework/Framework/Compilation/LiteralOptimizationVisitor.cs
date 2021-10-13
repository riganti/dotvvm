using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Binding;
using System;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    /// <summary> Merges subsequent literals into one and sets HtmlGenericControl.InnerText property when appropriate </summary>
    class LiteralOptimizationVisitor : ResolvedControlTreeVisitor
    {
        public override void VisitControl(ResolvedControl control)
        {
            OptimizeList(control.Content);

            var type = control.Metadata.Type;

            var textProperty =
                type == typeof(HtmlGenericControl) ? HtmlGenericControl.InnerTextProperty :
                typeof(ButtonBase).IsAssignableFrom(type) ? ButtonBase.TextProperty :
                typeof(RouteLink).IsAssignableFrom(type) ? RouteLink.TextProperty :
                typeof(CheckableControlBase).IsAssignableFrom(type) ? CheckableControlBase.TextProperty :
                null;

            // put literal into the Text property
            if (textProperty is not null &&
                !control.Properties.ContainsKey(textProperty) &&
                control.Content.Count == 1 &&
                IsOptimizableLiteral(control.Content[0], out var textBinding))
            {
                control.Content.Clear();
                control.SetProperty(new ResolvedPropertyBinding(
                    textProperty,
                    new ResolvedBinding(textBinding.Binding.GetProperty<ExpectedAsStringBindingExpression>().Binding)));
            }

            base.VisitControl(control);
        }

        public override void VisitPropertyControlCollection(ResolvedPropertyControlCollection propertyControlCollection)
        {
            OptimizeList(propertyControlCollection.Controls);
            base.VisitPropertyControlCollection(propertyControlCollection);
        }

        public override void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate)
        {
            OptimizeList(propertyTemplate.Content);
            base.VisitPropertyTemplate(propertyTemplate);
        }

        void OptimizeList(List<ResolvedControl> controls)
        {
            if (TryMergeLiterals(controls.ToArray()) is {} newLiteral)
            {
                controls.Clear();
                controls.Add(newLiteral);
            }
        }
        bool IsOptimizableLiteral(ResolvedControl c, [NotNullWhen(true)] out ResolvedBinding? textBinding)
        {
            textBinding = (c.Properties.GetValueOrDefault(Literal.TextProperty) as ResolvedPropertyBinding)?.Binding;
            if (textBinding is null || c.Metadata.Type != typeof(Literal) || textBinding.Errors.HasErrors)
                return false;
            // RenderSpanElement must be false
            if (c.Properties.GetValueOrDefault(Literal.RenderSpanElementProperty) is not ResolvedPropertyValue { Value: false })
                return false;
            // no other properties than RenderSpanElement and Text are allowed
            if (c.Properties.Count(p => p.Key.DeclaringType.IsAssignableFrom(typeof(Literal))) > 2)
                return false;
            return true;
        }

        ResolvedControl? TryMergeLiterals(ResolvedControl[] controls)
        {
            if (controls.Length <= 1)
                return null;
            if (!controls.All(c => c.Metadata.Type == typeof(RawLiteral) || c.Metadata.Type == typeof(Literal)))
                return null;
            if (controls.All(c => c.Metadata.Type == typeof(RawLiteral)))
            {
                // merge RawLiterals
                var text = "";
                var unencodedText = "";
                var isWhitespace = true;
                foreach (var c in controls)
                {
                    text += c.ConstructorParameters![0];
                    unencodedText += c.ConstructorParameters![1];
                    isWhitespace &= (bool)c.ConstructorParameters![2];
                }
                return new ResolvedControl(
                    controls[0].Metadata,
                    controls[0].DothtmlNode,
                    controls[0].DataContextTypeStack
                ) { ConstructorParameters = new object[] { text, unencodedText, isWhitespace } };
            }
            else
            {
                var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                var firstLiteral =
                    controls.FirstOrDefault(c => c.Metadata.Type == typeof(Literal));
                var firstBinding =
                    controls.Select(c => c.TryGetProperty(Literal.TextProperty, out var v) ? v : null)
                            .OfType<ResolvedPropertyBinding>()
                            .FirstOrDefault()?.Binding;
                if (firstBinding is null || firstLiteral is null)
                    return null;
                Expression text = Expression.Constant("");
                foreach (var c in controls)
                {
                    if (c.Metadata.Type == typeof(RawLiteral))
                    {
                        text = Expression.Add(text, Expression.Constant(c.ConstructorParameters![1]), concatMethod);
                    }
                    else
                    {
                        if (!IsOptimizableLiteral(c, out var textBinding) ||
                            textBinding.ParserOptions.BindingType != firstBinding.ParserOptions.BindingType ||
                            textBinding.DataContextTypeStack != firstBinding.DataContextTypeStack)
                        {
                            return null;
                        }

                        text = Expression.Add(text,
                            textBinding.Binding.GetProperty<ExpectedAsStringBindingExpression>().Binding
                                .GetProperty<CastedExpressionBindingProperty>().Expression,
                                concatMethod);
                    }
                }

                var newTextBinding = new ResolvedPropertyBinding(Literal.TextProperty, firstBinding.WithDifferentExpression(text));

                var newLiteral = new ResolvedControl(
                    firstLiteral.Metadata,
                    firstLiteral.DothtmlNode,
                    firstLiteral.DataContextTypeStack
                );
                newLiteral.SetProperty(newTextBinding);
                newLiteral.SetProperty(new ResolvedPropertyValue(Literal.RenderSpanElementProperty, false));
                return newLiteral;
            }
        }
    }
}
