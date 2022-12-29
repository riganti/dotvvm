using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls;
using System.Diagnostics;
using DotVVM.Framework.Compilation.ViewCompiler;
using DotVVM.Framework.Utils;
using System.Diagnostics.CodeAnalysis;
using FastExpressionCompiler;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Compilation
{
    public class BindingCompiler : IBindingCompiler
    {
        public static readonly ParameterExpression CurrentControlParameter = Expression.Parameter(typeof(DotvvmBindableObject), "currentControl");

        protected readonly DotvvmConfiguration configuration;
        protected readonly BindingCompilationService bindingService;

        public BindingCompiler(DotvvmConfiguration configuration, BindingCompilationService bindingService)
        {
            this.configuration = configuration;
            this.bindingService = bindingService;
        }

        public static Expression ReplaceParameters(Expression expression, DataContextStack? dataContext, IBinding contextObject, bool assertAllReplaced = true)
        {
            var visitor = new ParameterReplacementVisitor(dataContext, assertAllReplaced);
            var expression2 = visitor.Visit(expression);
            return visitor.WrapExpression(expression2, contextObject);
        }

        internal class ParameterReplacementVisitor: ExpressionVisitor
        {
            private readonly Dictionary<DataContextStack, int> ContextMap;
            private readonly DataContextStack? DataContext;
            private readonly bool AssertAllReplaced;
            private readonly HashSet<ParameterExpression> contextParameters = new HashSet<ParameterExpression>();

            private readonly Dictionary<DataContextStack, ParameterExpression> viewModelParameters = new();
            private readonly Dictionary<DataContextStack, ParameterExpression> controlParameters = new();

            public ParameterReplacementVisitor(DataContextStack? dataContext, bool assertAllReplaced = true)
            {
                this.DataContext = dataContext;
                this.ContextMap =
                    (dataContext?.EnumerableItems() ?? Enumerable.Empty<DataContextStack>())
                        .Select((a, i) => (a, i))
                        .ToDictionary(a => a.Item1, a => a.Item2);
                this.AssertAllReplaced = assertAllReplaced;
            }

            private int? FindIndex(DataContextStack context)
            {
                if (this.ContextMap.TryGetValue(context, out var result))
                    return result;
                return null;
            }

            private ParameterExpression GetControlParameter(DataContextStack? context)
            {
                if (context is null || context == this.DataContext)
                    return CurrentControlParameter;

                if (controlParameters.TryGetValue(context, out var result))
                    return result;

                var name = this.ContextMap.TryGetValue(context, out var index) ? index.ToString() : context.DataContextType.Name;
                return controlParameters[context] = Expression.Parameter(typeof(DotvvmBindableObject), $"control{name}");
            }

            private ParameterExpression GetViewModelParameter(DataContextStack context)
            {
                if (viewModelParameters.TryGetValue(context, out var result))
                    return result;

                var name = FindIndex(context) switch {
                    
                    0 => "vm_this",
                    1 => "vm_parent",
                    int n => "vm_parent" + n,
                    null => "vm_" + context.DataContextType.Name
                };
                return viewModelParameters[context] = Expression.Parameter(context.DataContextType, name);
            }

            [return: NotNullIfNotNull("node")]
            public override Expression? Visit(Expression? node)
            {
                if (node?.GetParameterAnnotation() is BindingParameterAnnotation ann)
                {
                    if (ann.ExtensionParameter != null)
                    {
                        var targetControl = GetControlParameter(ann.DataContext);
                        return ann.ExtensionParameter.GetServerEquivalent(targetControl);
                    }
                    else
                    {
                        var dc = ann.DataContext.NotNull("Invalid BindingParameterAnnotation");
                        return GetViewModelParameter(dc);
                    }
                }
                return base.Visit(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> expr)
            {
                var currentParameters = expr.Parameters.Where(contextParameters.Add).ToList();
                try {
                    return base.VisitLambda(expr);
                }
                finally {
                    Debug.Assert(currentParameters.TrueForAll(contextParameters.Remove));
                }
            }

            protected override Expression VisitBlock(BlockExpression expr)
            {
                var currentParameters = expr.Variables.Where(contextParameters.Add).ToList();
                try {
                    return base.VisitBlock(expr);
                }
                finally {
                    Debug.Assert(currentParameters.TrueForAll(contextParameters.Remove));
                }
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (AssertAllReplaced && node != CurrentControlParameter && !contextParameters.Contains(node))
                    throw new Exception($"Parameter {node.Name}:{node.Type.Name} could not be translated.");
                return base.VisitParameter(node);
            }

            /// <summary> Data context variables introduced by the visitor. The variables are all the `_parentX` and parent controls referenced in the binding. </summary>
            public IEnumerable<ParameterExpression> IntroducedVariables => viewModelParameters.Values.Concat(controlParameters.Values);


            /// <summary> Wraps the expression in a block which declared and initializes all the <see cref="IntroducedVariables" /> </summary>
            public Expression WrapExpression(Expression expression, IBinding contextObject)
            {
                var variables = IntroducedVariables.ToArray();
                if (variables.Length == 0)
                    return expression;

                // we only use this function after ConvertToObject or in update delegate which returns void
                Debug.Assert(expression.Type == typeof(void) || !expression.Type.IsValueType);

                var returnLabel = Expression.Label(expression.Type);
                var returnNull = Expression.Goto(returnLabel, Expression.Default(expression.Type));

                var initializer = GenerateInitializer(contextObject, returnNull);
                return Expression.Block(
                    variables,
                    initializer,
                    Expression.Label(returnLabel, defaultValue: expression));
            }

            /// <summary> Generates block which initializes the needed data context parameters (<see cref="IntroducedVariables" />) </summary>
            public BlockExpression GenerateInitializer(IBinding contextObject, Expression returnNull)
            {
                var result = new List<Expression>();
                var tempVariables = new List<ParameterExpression>();
                var contexts =
                    viewModelParameters.Keys.Concat(controlParameters.Keys).Distinct().OrderBy(cx => FindIndex(cx) ?? -1).ToList();

                ParameterExpression tempVariable(string name, Expression init)
                {
                    var v = Expression.Variable(init.Type, name);
                    tempVariables.Add(v);
                    result.Add(Expression.Assign(v, init));
                    return v;
                }

                void assignViewModel(ParameterExpression vmVariable, ParameterExpression tuple)
                {
                    result.Add(Expression.IfThen(
                        Expression.Field(tuple, "Item3"),
                        returnNull
                    ));
                    result.Add(Expression.Assign(vmVariable, Expression.Field(tuple, "Item2")));
                }

                var lastContextIndex = -1;
                Expression? lastContextControl = null;
                foreach (var cx in contexts)
                {
                    var cxIndex = FindIndex(cx);
                    Debug.Assert(cxIndex != lastContextIndex);
                    var controlVariable = controlParameters.GetValueOrDefault(cx);
                    var viewModelVariable = viewModelParameters.GetValueOrDefault(cx);

                    if (cxIndex == null)
                    {
                        // bindings without DataContext set
                        // this should be rare, so we can just use BindingHelper.FindDataContextTarget for each data context
                        var controlExpression = ExpressionUtils.Replace(
                                (DotvvmBindableObject control) => BindingHelper.FindDataContextTarget(control, cx, contextObject).target,
                                CurrentControlParameter
                            ).OptimizeConstants();

                        if (controlVariable is {})
                            result.Add(Expression.Assign(controlVariable, controlExpression));

                        if (viewModelVariable is {})
                        {
                            var control = controlVariable ?? controlExpression;
                            var tuple = tempVariable("tuple_" + cx.DataContextType.Name, getContextAndControl(0, control, cx));
                            assignViewModel(viewModelVariable, tuple);
                        }
                    }
                    else
                    {
                        // We aim to build a chain of GetContextControl / GetContextAndControl invocations.
                        // This should traverse the DataContext hierarchy only once, and only to the required depth.
                        // The binding in DataContext property evaluated only if the data context is used in the current binding.
                        var (baseControl, skip) = lastContextControl switch {
                            null => (CurrentControlParameter, cxIndex.Value),
                            _ => (
                                (Expression)Expression.Property(lastContextControl, "Parent"),
                                cxIndex.Value - lastContextIndex - 1
                            )
                        };
                        lastContextIndex = cxIndex.Value;
                        if (viewModelVariable is null)
                        {
                            var control = getContextControl(skip, baseControl, cx);
                            result.Add(Expression.Assign(controlVariable.NotNull(), control));
                            lastContextControl = controlVariable;
                        }
                        else
                        {
                            var tuple = tempVariable("tuple" + cxIndex, getContextAndControl(skip, baseControl, cx));
                            assignViewModel(viewModelVariable, tuple);
                            if (controlVariable is {})
                            {
                                result.Add(Expression.Assign(controlVariable, Expression.Field(tuple, "Item1")));
                                lastContextControl = controlVariable;
                            }
                            else
                            {
                                lastContextControl = Expression.Field(tuple, "Item1");
                            }
                        }
                    }
                }
                return Expression.Block(tempVariables, result);

                Expression getContextAndControl(int skip, Expression control, DataContextStack x) =>
                    Expression.Call(
                        typeof(CodegenHelpers),
                        nameof(CodegenHelpers.GetContextAndControl),
                        new Type[] { x.DataContextType },
                        Expression.Constant(skip),
                        control,
                        Expression.Constant(new ErrorInfo(contextObject, x, FindIndex(x))),
                        CurrentControlParameter
                    );
                Expression getContextControl(int skip, Expression control, DataContextStack x) =>
                    Expression.Call(
                        typeof(CodegenHelpers),
                        nameof(CodegenHelpers.GetContextControl),
                        Type.EmptyTypes,
                        Expression.Constant(skip),
                        control,
                        Expression.Constant(new ErrorInfo(contextObject, x, FindIndex(x))),
                        CurrentControlParameter
                    );
            }

            static class CodegenHelpers
            {
                // public static DotvvmBindableObject GetContextControl(DotvvmBindableObject? control, ErrorInfo errorInfo, DotvvmBindableObject evaluatingControl)
                // {
                //     while (control != null)
                //     {
                //         if (control.properties.Contains(DotvvmBindableObject.DataContextProperty))
                //             return control;
                //         control = control.Parent;
                //     }
                //     ThrowNotEnoughDataContexts(errorInfo, evaluatingControl);
                //     return null!;
                // }

                /// <summary> Returns the nearest ancestor control with DataContext property set, after skipping `skip` such ancestors. </summary>
                public static DotvvmBindableObject GetContextControl(int skip, DotvvmBindableObject? control, ErrorInfo errorInfo, DotvvmBindableObject evaluatingControl)
                {
                    while (control != null)
                    {
                        if (control.properties.Contains(DotvvmBindableObject.DataContextProperty))
                        {
                            if (skip == 0)
                                return control;
                            skip--;
                        }
                        control = control.Parent;
                    }
                    ThrowNotEnoughDataContexts(errorInfo, evaluatingControl);
                    return null!;
                }


                /// <summary> Returns the nearest ancestor control with DataContext property set, after skipping `skip` such ancestors. Includes the DataContext value </summary>
                public static (DotvvmBindableObject control, T context, bool isNull) GetContextAndControl<T>(int skip, DotvvmBindableObject? control, ErrorInfo errorInfo, DotvvmBindableObject evaluatingControl)
                {
                    while (control != null)
                    {
                        if (control.properties.TryGet(DotvvmBindableObject.DataContextProperty, out var contextRaw))
                        {
                            if (skip == 0)
                            {
                                var context = control.EvalPropertyValue(DotvvmBindableObject.DataContextProperty, contextRaw);
                                if (context is T contextT)
                                    return (control, contextT, false);
                                if (context is null)
                                    return (control, default!, true);
                                ThrowWrongContextType(errorInfo, context, evaluatingControl);
                            }
                            skip--;
                        }
                        control = control.Parent;
                    }
                    ThrowNotEnoughDataContexts(errorInfo, evaluatingControl);
                    return default;
                }

                [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
                static void ThrowNotEnoughDataContexts(ErrorInfo errorInfo, DotvvmBindableObject evaluatingControl)
                {
                    throw new NotEnoughDataContextsException(errorInfo.DataContext, errorInfo.Index!.Value, errorInfo.Binding, evaluatingControl);
                }

                [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
                static void ThrowWrongContextType(ErrorInfo errorInfo, object? receivedObject, DotvvmBindableObject evaluatingControl)
                {
                    throw new WrongDataContextTypeException(errorInfo.DataContext, receivedObject?.GetType(), errorInfo.Index, errorInfo.Binding, evaluatingControl);
                }
            }

            sealed record ErrorInfo(
                IBinding Binding,
                DataContextStack DataContext,
                int? Index
            )
            { }

            sealed record NotEnoughDataContextsException(DataContextStack MissingDataContext, int DataContextIndex, IBinding RelatedBinding, DotvvmBindableObject RelatedControl): DotvvmExceptionBase(RelatedBinding: RelatedBinding, RelatedControl: RelatedControl)
            {
                public override string Message => $"Could not evaluate binding {RelatedBinding!.ToString()}, " + 
                    $"data context {DataContextIndex switch { 0 => "_this", 1 => "_parent", var n => "_parent"+n }}: {MissingDataContext.DataContextType.ToCode(stripNamespace: true)} does not exist. " +
                    $"Control has the following contexts: {string.Join(", ", RelatedControl!.GetDataContexts().Select(c => c?.GetType().ToCode(stripNamespace: true) ?? "?"))}";
            }

            sealed record WrongDataContextTypeException(DataContextStack ExpectedDataContext, Type? ReceivedType, int? DataContextIndex, IBinding RelatedBinding, DotvvmBindableObject RelatedControl): DotvvmExceptionBase(RelatedBinding: RelatedBinding, RelatedControl: RelatedControl)
            {
                public override string Message => $"Could not evaluate binding {RelatedBinding!.ToString()}, " + 
                    $"data context {DataContextIndex switch { null => "?", 0 => "_this", 1 => "_parent", var n => "_parent"+n }}: {ExpectedDataContext.DataContextType.ToCode()} was expected, but got {ReceivedType?.ToCode() ?? "null"}. " +
                    $"Control has the following contexts: {string.Join(", ", RelatedControl!.GetDataContexts().Select(c => c?.GetType().ToCode(stripNamespace: true) ?? "?"))}";
            }

        }

        public virtual IBinding CreateMinimalClone(IBinding binding)
        {
            object?[] properties = GetMinimalCloneProperties(binding).ToArray();

            for (int i = 0; i < properties.Length; i++)
            {
                var p = properties[i];
                if (p is null) continue;

                if (p is DataSourceAccessBinding dataSource)
                    properties[i] = cloneNestedBinding(dataSource.Binding)?.Apply(b => new DataSourceAccessBinding(b));
                if (p is DataSourceLengthBinding dataLength)
                    properties[i] = cloneNestedBinding(dataLength.Binding)?.Apply(b => new DataSourceLengthBinding(b));
                if (p is DataSourceCurrentElementBinding collectionElement)
                    properties[i] = cloneNestedBinding(collectionElement.Binding)?.Apply(b => new DataSourceCurrentElementBinding(b));
                if (p is SelectorItemBindingProperty selectorItem)
                    properties[i] = cloneNestedBinding(selectorItem.Expression)?.Apply(b => new SelectorItemBindingProperty(b));
                if (p is ThisBindingProperty thisBinding)
                    properties[i] = cloneNestedBinding(thisBinding.binding)?.Apply(b => new ThisBindingProperty(b));
            }

            return (IBinding)Activator.CreateInstance(binding.GetType(), new object[] {
                bindingService,
                properties
            })!;

            T? cloneNestedBinding<T>(T b)
                where T: class, IBinding =>
                b == binding ? null : // it it's self, then we can just recreate it at runtime
                (T)CreateMinimalClone(b);
        }

        public IEnumerable<object> GetMinimalCloneProperties(IBinding binding)
        {
            var requirements = bindingService.GetRequirements(binding);
            return requirements.Required.Concat(requirements.Optional)
                    .Concat(new[] { typeof(ParsedExpressionBindingProperty), typeof(OriginalStringBindingProperty), typeof(DataContextStack), typeof(DotvvmLocationInfo), typeof(BindingParserOptions), typeof(BindingCompilationRequirementsAttribute), typeof(ExpectedTypeBindingProperty), typeof(AssignedPropertyBindingProperty) })
                    .Select(p => binding.GetProperty(p, ErrorHandlingMode.ReturnNull))
                    .Where(p => p != null).ToArray()!;
        }

        public virtual Expression EmitCreateBinding(DefaultViewCompilerCodeEmitter emitter, ResolvedBinding binding)
        {
            var newbinding = CreateMinimalClone(binding.Binding);
            return emitter.EmitValue(newbinding);
        }

        private T CompileExpression<T>(Expression<T> expression, DebugInfoExpression debugInfo)
        {
            if (!configuration.Debug || !configuration.AllowBindingDebugging || debugInfo == null)
            {
                return expression.Compile();
            }
            else
            {
                throw new NotImplementedException();
                //try
                //{
                //    var visitor = new DebugInfoExpressionVisitor { DebugInfo = debugInfo };
                //    expression = visitor.Visit(expression) as Expression<T>;

                //    var pdb = DebugInfoGenerator.CreatePdbGenerator();
                //    //return expression.Compile(pdb);
                //    var type = moduleBuilder.Value.DefineType("bindingWrapperType" + Interlocked.Increment(ref bindingClassCtr));
                //    var method = type.DefineMethod("Method", MethodAttributes.Public | MethodAttributes.Static);
                //    expression.CompileToMethod(method, pdb);
                //    var bakedType = type.CreateType();
                //    return (T)(object)bakedType.GetMethods().First().CreateDelegate(typeof(T));
                //}
                //catch
                //{
                //    return expression.Compile();
                //}
            }
        }

        class DebugInfoExpressionVisitor : ExpressionVisitor
        {
            public DebugInfoExpression DebugInfo { get; }

            public DebugInfoExpressionVisitor(DebugInfoExpression debugInfo)
            {
                DebugInfo = debugInfo;
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                node = node.Update(Expression.Block(DebugInfo, node.Body), node.Parameters);
                return base.VisitLambda<T>(node);
            }
        }
    }
}
