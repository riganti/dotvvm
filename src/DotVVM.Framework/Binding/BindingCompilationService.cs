using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Binding
{
    public class BindingCompilationOptions
    {
        public List<object> TransformerClasses { get; set; } = new List<object>();
    }

    public class BindingCompilationService
    {
        private readonly IExpressionToDelegateCompiler expressionCompiler;
        private readonly Lazy<BindingCompilationService> noInitService;
        public DotvvmBindingCacheHelper Cache { get; }

        public BindingCompilationService(IOptions<BindingCompilationOptions> options, IExpressionToDelegateCompiler expressionCompiler, IDotvvmCacheAdapter cache)
        {
            this.expressionCompiler = expressionCompiler;
            this.noInitService = new Lazy<BindingCompilationService>(() => new NoInitService(options, expressionCompiler, cache));
            foreach (var p in GetDelegates(options.Value.TransformerClasses))
                resolvers.AddDelegate(p);
            this.Cache = new DotvvmBindingCacheHelper(cache, this);
        }

        BindingResolverCollection resolvers = new BindingResolverCollection(Enumerable.Empty<Delegate>());
        [ThreadStatic]
        private static bool LookingForResolvers = false;

        private BindingResolverCollection GetAdditionalResolvers(IBinding binding)
        {
            if (LookingForResolvers) return null;
            try
            {
                LookingForResolvers = true;
                return binding.GetProperty<BindingResolverCollection>(ErrorHandlingMode.ReturnNull);
            }
            finally
            {
                LookingForResolvers = false;
            }
        }

        public virtual object ComputeProperty(Type type, IBinding binding)
        {
            if (type == typeof(BindingCompilationService)) return this;
            if (type.IsAssignableFrom(binding.GetType())) return binding;

            var additionalResolvers = GetAdditionalResolvers(binding);
            var bindingResolvers = GetResolversForBinding(binding.GetType());

            var resolver = additionalResolvers?.FindResolver(type) ??
                bindingResolvers?.FindResolver(type) ??
                this.resolvers.FindResolver(type);

            object getParameterValue(ParameterInfo p) => binding.GetProperty(p.ParameterType, p.HasDefaultValue ? ErrorHandlingMode.ReturnNull : ErrorHandlingMode.ReturnException) ?? p.DefaultValue;

            Exception checkArguments(object[] arguments) =>
                arguments.OfType<Exception>().ToArray() is var exceptions && exceptions.Any() ?
                new BindingPropertyException(binding, type, "unresolvable arguments", exceptions) :
                null;

            if (resolver != null)
            {
                var arguments = resolver.GetMethodInfo().GetParameters().Select(getParameterValue).ToArray();
                { if (checkArguments(arguments) is Exception exc) return exc; }
                var value = resolver.ExceptionSafeDynamicInvoke(arguments);
                // post process the value
                foreach (var postProcessor in this.resolvers.GetPostProcessors(type)
                    .Concat(bindingResolvers.GetPostProcessors(type)
                    .Concat(additionalResolvers?.GetPostProcessors(type) ?? Enumerable.Empty<Delegate>())))
                {
                    var method = postProcessor.GetMethodInfo();
                    arguments = new[] { value }.Concat(method.GetParameters().Skip(1).Select(getParameterValue)).ToArray();
                    if (checkArguments(arguments) is Exception exc) return exc;
                    value = postProcessor.ExceptionSafeDynamicInvoke(arguments) ?? value;
                }
                return value ?? new BindingPropertyException(binding, type, "resolver returned null");
            }
            if (typeof(Delegate).IsAssignableFrom(type))
            {
                var result = ComputeProperty(typeof(Expression<>).MakeGenericType(type), binding);
                if (result is LambdaExpression lambda)
                    return expressionCompiler.Compile(lambda);
                else return result;
            }
            else return new BindingPropertyException(binding, type, "resolver not found"); // don't throw the exception, since it creates noise for debugger
        }

        protected Exception GetException(IBinding binding, string message) =>
            binding.GetProperty<ResolvedBinding>(ErrorHandlingMode.ReturnNull) is ResolvedBinding resolvedBinding ?
                new DotvvmCompilationException(message, resolvedBinding.DothtmlNode.Tokens) :
            binding.GetProperty<LocationInfoBindingProperty>(ErrorHandlingMode.ReturnNull) is LocationInfoBindingProperty locationInfo ?
                new DotvvmControlException(message, null, locationInfo.ControlType, locationInfo.LineNumber, locationInfo.FileName, locationInfo.Ranges) :
            new Exception(null);

        ConcurrentDictionary<Type, BindingResolverCollection> bindingResolverCache = new ConcurrentDictionary<Type, BindingResolverCollection>();
        BindingResolverCollection GetResolversForBinding(Type bindingType)
        {
            return bindingResolverCache.GetOrAdd(bindingType, t =>
                new BindingResolverCollection(t.GetTypeInfo().GetCustomAttributes<BindingCompilationOptionsAttribute>(true)
                .SelectMany(o => o.GetResolvers())));
        }

        ConcurrentDictionary<Type, BindingCompilationRequirementsAttribute> defaultRequirementCache = new ConcurrentDictionary<Type, BindingCompilationRequirementsAttribute>();
        protected BindingCompilationRequirementsAttribute GetDefaultRequirements(Type bindingType)
        {
            return defaultRequirementCache.GetOrAdd(bindingType, t =>
                t.GetTypeInfo().GetCustomAttributes<BindingCompilationRequirementsAttribute>(inherit: true).Aggregate((a, b) => a.ApplySecond(b)));
        }

        public BindingCompilationRequirementsAttribute GetRequirements(IBinding binding, IEnumerable<BindingCompilationRequirementsAttribute> bindingRequirements = null)
        {
            var requirements = GetDefaultRequirements(binding.GetType());
            if (bindingRequirements != null) foreach (var req in bindingRequirements) requirements = requirements.ApplySecond(req);
            if (binding.GetProperty<BindingCompilationRequirementsAttribute>(ErrorHandlingMode.ReturnNull) is var second && second != null)
                requirements = requirements.ApplySecond(second);
            return requirements;
        }

        /// <summary>
        /// Resolves required and optional properties
        /// </summary>
        public virtual void InitializeBinding(IBinding binding, IEnumerable<BindingCompilationRequirementsAttribute> bindingRequirements = null)
        {
            InitializeBindingCore(binding, GetRequirements(binding, bindingRequirements));
        }

        protected static void InitializeBindingCore(IBinding binding, BindingCompilationRequirementsAttribute bindingRequirements)
        {
            var reporter = binding.GetProperty<BindingErrorReporterProperty>(ErrorHandlingMode.ReturnNull);
            var throwException = reporter == null;
            reporter = reporter ?? new BindingErrorReporterProperty();
            foreach (var req in bindingRequirements.Required)
            {
                if (binding.GetProperty(req, ErrorHandlingMode.ReturnException) is Exception error)
                    reporter.Errors.Push((req, error, DiagnosticSeverity.Error));
            }
            if (throwException && reporter.HasErrors)
                throw new AggregateException(reporter.GetErrorMessage(binding), reporter.Exceptions);
        }

        public static Delegate[] GetDelegates(IEnumerable<object> objects) => (
            from t in objects
            from m in t.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            where m.DeclaringType != typeof(object)
            select t is Delegate ? (Delegate)t : m.CreateDelegate(MethodGroupExpression.GetDelegateType(m), t)
        ).ToArray();

        class NoInitService : BindingCompilationService
        {
            public NoInitService(IOptions<BindingCompilationOptions> options, IExpressionToDelegateCompiler expressionCompiler, IDotvvmCacheAdapter cache) : base(options, expressionCompiler, cache) { }

            public override void InitializeBinding(IBinding binding, IEnumerable<BindingCompilationRequirementsAttribute> bindingRequirements = null)
            {
                // no-op
            }
        }

        public BindingCompilationService WithoutInitialization() => this.noInitService.Value;
    }

    public sealed class BindingResolverCollection
    {
        private readonly ConcurrentDictionary<Type, Delegate> resolvers = new ConcurrentDictionary<Type, Delegate>();
        private readonly ConcurrentDictionary<Type, ConcurrentStack<Delegate>> postProcs = new ConcurrentDictionary<Type, ConcurrentStack<Delegate>>();

        public IEnumerable<Delegate> Delegates => resolvers.Values.Concat(postProcs.Values.SelectMany(_ => _));

        public BindingResolverCollection(IEnumerable<Delegate> delegates)
        {
            foreach (var d in delegates) AddDelegate(d, replace: true);
        }

        public void AddResolver(Delegate resolver, bool replace = false)
        {
            if (replace) resolvers[resolver.GetMethodInfo().ReturnType] = resolver;
            else if (!resolvers.TryAdd(resolver.GetMethodInfo().ReturnType, resolver))
                throw new NotSupportedException($"Can't insert more resolvers for property of type '{resolver.GetMethodInfo().ReturnType}'.");
        }

        public void AddPostProcessor(Delegate processor)
        {
            var method = processor.GetMethodInfo();
            var type = method.GetParameters().First().ParameterType;
            if (method.ReturnType != typeof(void) && method.ReturnType != type)
                throw new Exception("Binding property post-processing function must return void or first parameter's type.");
            var list = postProcs.GetOrAdd(type, _ => new ConcurrentStack<Delegate>());
            list.Push(processor);
        }

        public void AddDelegate(Delegate func, bool replace = false)
        {
            var method = func.GetMethodInfo();
            var type = method.GetParameters().FirstOrDefault()?.ParameterType;
            if (method.ReturnType == typeof(void) || method.ReturnType == type)
                AddPostProcessor(func);
            else AddResolver(func, replace);
        }

        public IEnumerable<Delegate> GetPostProcessors(Type type) =>
            postProcs.TryGetValue(type, out var result) ? result : Enumerable.Empty<Delegate>();

        public Delegate FindResolver(Type type) =>
            resolvers.TryGetValue(type, out var result) ? result : null;
    }
}
