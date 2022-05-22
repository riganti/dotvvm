using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.RegularExpressions;
using DotVVM.AutoUI.Annotations;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.Metadata
{
    public static class ConditionalFieldBindingProvider
    {
        public static ValueOrBinding<bool> IsEnabledBinding(this PropertyDisplayMetadata property, AutoUIContext context) =>
            ConditionalFieldBindingProvider.GetPropertyBinding(property.EnabledAttributes, context)
                .And(new ValueOrBinding<bool>(property.IsEditable));

        public static ValueOrBinding<bool> GetPropertyBinding(IEnumerable<IConditionalFieldAttribute> attributes, AutoUIContext context)
        {
            var result = new ValueOrBinding<bool>(true);

            foreach (var attribute in attributes)
            {
                var currentUserParam = Expression.Parameter(typeof(ClaimsPrincipal))
                    .AddParameterAnnotation(new BindingParameterAnnotation(extensionParameter: new CurrentUserExtensionParameter()));
                 
                result = result.And(BuildExpression(context, currentUserParam, attribute));
            }

            return result;
        }
        

        private static ValueOrBinding<bool> BuildExpression(AutoUIContext context, Expression currentUserParam, IConditionalFieldAttribute attribute)
        {
            var value = new ValueOrBinding<bool>(true);

            // view names doesn't need to be filtered here

            if (!string.IsNullOrEmpty(attribute.Roles))
            {
                value = value.And(
                    ProcessExpression(attribute.Roles, i => {
                        var binding = new ResourceBindingExpression<bool>(
                            context.BindingService,
                            new object[] {
                                new ParsedExpressionBindingProperty(
                                    ExpressionUtils.Replace(
                                        (ClaimsPrincipal p, string r) => p.IsInRole(r),
                                        currentUserParam,
                                        Expression.Constant(i))
                                    ),
                                context.DataContextStack
                            });
                        return new ValueOrBinding<bool>(binding);
                    }));
            }

            if (attribute.IsAuthenticated != AuthenticationMode.Any)
            {
                var isAuthenticated = attribute.IsAuthenticated == AuthenticationMode.Authenticated;

                var binding = new ResourceBindingExpression<bool>(
                    context.BindingService,
                    new object[]
                    {
                        new ParsedExpressionBindingProperty(
                            ExpressionUtils.Replace(
                                (ClaimsPrincipal p) => p.Identity.IsAuthenticated == isAuthenticated,
                                currentUserParam
                            )
                        ),
                        context.DataContextStack
                    });
                value = value.And(new ValueOrBinding<bool>(binding));
            }

            return value;
        }

        internal static ValueOrBinding<bool> ProcessExpression(string viewName, Func<string, ValueOrBinding<bool>> processor)
        {
            ValueOrBinding<bool> ProcessPart(string identifier, string negate)
            {
                if (!Regex.IsMatch(identifier, "^!?[a-zA-Z_][a-zA-Z0-9_]*$"))
                {
                    throw new DotvvmControlException($"Invalid syntax in the attribute ViewNames property near syntax '{identifier}'! The expression can only contain identifier names and operators & (AND), | (OR) and ! (NOT).");
                }

                var result = processor(identifier);
                if (negate == "!")
                {
                    result = result.Negate();
                }
                return result;
            }

            var match = Regex.Match(viewName, @"^\s*(!?)(\w+)(\s*(\|{1,2}|&{1,2})\s*(!?)(\w+)\s*)*$");
            if (!match.Success)
            {
                throw new DotvvmControlException($"Invalid syntax in the attribute ViewNames property '{viewName}'! The expression can only contain identifier names and operators & (AND), | (OR) and ! (NOT).");
            }
            var negate = match.Groups[1].Value;
            var identifier = match.Groups[2].Value;
            var result = ProcessPart(identifier, negate);

            for (var i = 0; i < match.Groups[3].Captures.Count; i++)
            {
                negate = match.Groups[5].Captures[i].Value;
                identifier = match.Groups[6].Captures[i].Value;

                var op = match.Groups[4].Captures[i].Value;
                var partResult = ProcessPart(identifier, negate);
                if (op[0] == '|')
                {
                    result = result.Or(partResult);
                }
                else
                {
                    result = result.And(partResult);
                }
            }

            return result;
        }

        internal static bool ProcessExpression(string? viewName, Func<string, bool> processor)
        {
            if (string.IsNullOrEmpty(viewName))
                return true;
            return ProcessExpression(viewName, s => new ValueOrBinding<bool>(processor(s))).GetValue();
        }
    }
}
