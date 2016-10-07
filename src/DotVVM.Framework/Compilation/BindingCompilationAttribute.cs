using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    public class BindingCompilationAttribute : Attribute
    {
        private static ConcurrentDictionary<Type, BindingCompilationRequirementsAttribute> requirementCache = new ConcurrentDictionary<Type, BindingCompilationRequirementsAttribute>();
        public virtual BindingCompilationRequirementsAttribute GetRequirements(Type bindingType)
        {
            return requirementCache.GetOrAdd(bindingType, _ =>
            {
                return bindingType.GetTypeInfo().GetCustomAttribute<BindingCompilationRequirementsAttribute>(true) ?? new BindingCompilationRequirementsAttribute
                {
                    Delegate = BindingCompilationRequirementType.IfPossible,
                    OriginalString = BindingCompilationRequirementType.IfPossible,
                    Expression = BindingCompilationRequirementType.IfPossible,
                    Javascript = BindingCompilationRequirementType.IfPossible,
                    ActionFilters = BindingCompilationRequirementType.IfPossible,
                    UpdateDelegate = BindingCompilationRequirementType.IfPossible
                };
            });
        }
        public virtual IEnumerable<ActionFilterAttribute> GetActionFilters(Expression expression)
        {
            var list = new List<ActionFilterAttribute>();
            expression.ForEachMember(m =>
            {
                list.AddRange(CustomAttributeExtensions.GetCustomAttributes<ActionFilterAttribute>((MemberInfo)m));
            });
            return list;
        }
        public virtual Expression<CompiledBindingExpression.BindingDelegate> CompileToDelegate(Expression binding, DataContextStack dataContext, Type expectedType)
        {
            var viewModelsParameter = Expression.Parameter(typeof(object[]), "vm");
            var controlRootParameter = Expression.Parameter(typeof(DotvvmBindableObject), "controlRoot");
            var expr = ExpressionUtils.Replace(ConvertExpressionToType(binding, expectedType), BindingCompiler.GetParameters(dataContext, viewModelsParameter, Expression.Convert(controlRootParameter, dataContext.RootControlType)));
            expr = ExpressionUtils.ConvertToObject(expr);
            return Expression.Lambda<CompiledBindingExpression.BindingDelegate>(expr, viewModelsParameter, controlRootParameter);
        }


        protected virtual Expression ConvertExpressionToType(Expression expr, Type expectedType)
            => TypeConversion.ImplicitConversion(expr, expectedType, throwException: true, allowToString: true);

        public virtual Expression<CompiledBindingExpression.BindingUpdateDelegate> CompileToUpdateDelegate(Expression binding, DataContextStack dataContext)
        {
            var viewModelsParameter = Expression.Parameter(typeof(object[]), "vm");
            var controlRootParameter = Expression.Parameter(typeof(DotvvmBindableObject), "controlRoot");
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var expr = ExpressionUtils.Replace(binding, BindingCompiler.GetParameters(dataContext, viewModelsParameter, Expression.Convert(controlRootParameter, dataContext.RootControlType)));
            var assignment = Expression.Assign(expr, Expression.Convert(valueParameter, expr.Type));
            return Expression.Lambda<CompiledBindingExpression.BindingUpdateDelegate>(assignment, viewModelsParameter, controlRootParameter, valueParameter);
        }

        public virtual Expression GetExpression(ResolvedBinding binding)
        {
            return binding.GetExpression();
        }

        public virtual string CompileToJavascript(ResolvedBinding binding, CompiledBindingExpression expression)
        {
            var javascript = JavascriptTranslator.CompileToJavascript(binding.GetExpression(), binding.DataContextTypeStack);

            if (javascript == "$data")
            {
                javascript = "$rawData";
            }
            else if (javascript.StartsWith("$data.", StringComparison.Ordinal))
            {
                javascript = javascript.Substring("$data.".Length);
            }

            // do not produce try/eval on single properties
            if (javascript.Contains(".") || javascript.Contains("("))
            {
                return "dotvvm.evaluator.tryEval(function(){return " + javascript + "})";
            }
            else
            {
                return javascript;
            }
        }
    }
}
