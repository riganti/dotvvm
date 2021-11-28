using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
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
            this.noInitService = 
                this is NoInitService ? new(() => this)
                                      : new(() => new NoInitService(options, expressionCompiler, cache));
            foreach (var p in GetDelegates(options.Value.TransformerClasses))
                resolvers.AddDelegate(p);
            this.Cache = new DotvvmBindingCacheHelper(cache, this);
        }

        BindingResolverCollection resolvers = new BindingResolverCollection(Enumerable.Empty<Delegate>());

        public virtual object? ComputeProperty(Type type, IBinding binding)
        {
            var additionalResolvers = binding.GetAdditionalResolvers();
            var bindingResolvers = GetResolversForBinding(binding.GetType());

            var resolver = additionalResolvers?.FindResolver(type) ??
                bindingResolvers.FindResolver(type) ??
                this.resolvers.FindResolver(type);

            object? getParameterValue(ParameterInfo p) => binding.GetProperty(p.ParameterType, p.HasDefaultValue ? ErrorHandlingMode.ReturnNull : ErrorHandlingMode.ReturnException) ?? p.DefaultValue;

            Exception? checkArguments(object?[] arguments) =>
                arguments.OfType<Exception>().ToArray() is var exceptions && exceptions.Any() ?
                BindingPropertyException.FromArgumentExceptions(binding, type, exceptions) :
                null;

            if (resolver != null)
            {
                var arguments = resolver.Method.GetParameters().Select(getParameterValue).ToArray();
                { if (checkArguments(arguments) is Exception exc) return exc; }
                var value = resolver.ExceptionSafeDynamicInvoke(arguments);
                // post process the value
                foreach (var postProcessor in this.resolvers.GetPostProcessors(type)
                    .Concat(bindingResolvers.GetPostProcessors(type)
                    .Concat(additionalResolvers?.GetPostProcessors(type) ?? Enumerable.Empty<Delegate>())))
                {
                    var method = postProcessor.Method;
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
            // instead of returning exception we return null since this is the most common exception
            // and whatever we return will probably stay in RAM forever.
            else return null;
            //; // don't throw the exception, since it creates noise for debugger
        }

        protected Exception GetException(IBinding binding, string message) =>
            binding.GetProperty<ResolvedBinding>(ErrorHandlingMode.ReturnNull) is ResolvedBinding resolvedBinding && resolvedBinding.DothtmlNode is object ?
                new DotvvmCompilationException(message, resolvedBinding.DothtmlNode.Tokens) :
            binding.GetProperty<DotvvmLocationInfo>(ErrorHandlingMode.ReturnNull) is DotvvmLocationInfo locationInfo ?
                new DotvvmControlException(message, null, locationInfo) :
            new Exception(message);

        ConcurrentDictionary<Type, BindingResolverCollection> bindingResolverCache = new ConcurrentDictionary<Type, BindingResolverCollection>();
        BindingResolverCollection GetResolversForBinding(Type bindingType)
        {
            return bindingResolverCache.GetOrAdd(bindingType, t =>
                new BindingResolverCollection(t.GetCustomAttributes<BindingCompilationOptionsAttribute>(true)
                .SelectMany(o => o.GetResolvers())));
        }

        ConcurrentDictionary<Type, BindingCompilationRequirementsAttribute> defaultRequirementCache = new ConcurrentDictionary<Type, BindingCompilationRequirementsAttribute>();
        protected BindingCompilationRequirementsAttribute GetDefaultRequirements(Type bindingType)
        {
            return defaultRequirementCache.GetOrAdd(bindingType, t =>
                t.GetCustomAttributes<BindingCompilationRequirementsAttribute>(inherit: true).Aggregate((a, b) => a.ApplySecond(b)));
        }

        public BindingCompilationRequirementsAttribute GetRequirements(IBinding binding, IEnumerable<BindingCompilationRequirementsAttribute>? bindingRequirements = null)
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
        public virtual void InitializeBinding(IBinding binding, IEnumerable<BindingCompilationRequirementsAttribute>? bindingRequirements = null)
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
            {
                var e = reporter.Errors.Where(e => e.severity == DiagnosticSeverity.Error).ToArray();
                throw BindingPropertyException.FromArgumentExceptions(
                    binding,
                    e[0].req,
                    e.Select(e => e.error).ToArray(),
                    isRequiredProperty: true
                );
            }
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

            public override void InitializeBinding(IBinding binding, IEnumerable<BindingCompilationRequirementsAttribute>? bindingRequirements = null)
            {
                // no-op
            }
        }

        public BindingCompilationService WithoutInitialization() => this.noInitService.Value;
    }

    public sealed class BindingResolverCollection
    {
        private ConcurrentDictionary<Type, Delegate>? resolvers = null;
        private ConcurrentDictionary<Type, ConcurrentStack<Delegate>>? postProcs = null;
        
        [MemberNotNull("resolvers")]
        void InitResolvers()
        {
            if (resolvers is null)
                // concurrencyLevel: 1, we don't need super high parallel performance, it better to save memory on all those locks
                Interlocked.CompareExchange(ref resolvers,  new ConcurrentDictionary<Type, Delegate>(concurrencyLevel: 1, capacity: 1), null);
        }
        [MemberNotNull("postProcs")]
        void InitPostProcs()
        {
            if (postProcs is null)
                Interlocked.CompareExchange(ref postProcs, new ConcurrentDictionary<Type, ConcurrentStack<Delegate>>(1, 1), null);
        }

        public IEnumerable<Delegate> Delegates =>
            resolvers is null && postProcs is null ? Enumerable.Empty<Delegate>() :
            (resolvers?.Values ?? Enumerable.Empty<Delegate>())
                .Concat(postProcs?.Values.SelectMany(_ => _) ?? Enumerable.Empty<Delegate>());

        public BindingResolverCollection(IEnumerable<Delegate> delegates)
        {
            foreach (var d in delegates) AddDelegate(d, replace: true);
        }

        public void AddResolver(Delegate resolver, bool replace = false)
        {
            InitResolvers();
            if (replace) resolvers[resolver.Method.ReturnType] = resolver;
            else if (!resolvers.TryAdd(resolver.Method.ReturnType, resolver))
                throw new NotSupportedException($"Can't insert more resolvers for property of type '{resolver.Method.ReturnType}'.");
        }

        public void AddPostProcessor(Delegate processor)
        {
            var method = processor.Method;
            var type = method.GetParameters().First().ParameterType;
            if (method.ReturnType != typeof(void) && method.ReturnType != type)
                throw new Exception("Binding property post-processing function must return void or first parameter's type.");
            InitPostProcs();
            var list = postProcs.GetOrAdd(type, _ => new ConcurrentStack<Delegate>());
            list.Push(processor);
        }

        public void AddDelegate(Delegate func, bool replace = false)
        {
            var method = func.Method;
            var type = method.GetParameters().FirstOrDefault()?.ParameterType;
            if (method.ReturnType == typeof(void) || method.ReturnType == type)
                AddPostProcessor(func);
            else AddResolver(func, replace);
        }

        public IEnumerable<Delegate> GetPostProcessors(Type type) =>
            postProcs?.TryGetValue(type, out var result) == true ? result : Enumerable.Empty<Delegate>();

        public Delegate? FindResolver(Type type) =>
            resolvers?.TryGetValue(type, out var result) == true ? result : null;
    }
}
