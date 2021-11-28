using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding.Expressions
{
    [BindingCompilationRequirements(optional: new[] { typeof(BindingResolverCollection) })]
    [Newtonsoft.Json.JsonConverter(typeof(BindingDebugJsonConverter))]
    public abstract class BindingExpression : IBinding, ICloneableBinding
    {
        private protected struct PropValue<TValue> where TValue : class
        {
            public TValue? Value;
            public Exception? Error;

            public object? GetValue(ErrorHandlingMode errorMode, IBinding contextBinding, Type propType)
            {
                if (Value is null && errorMode != ErrorHandlingMode.ReturnNull)
                {
                    var err = Error ?? new BindingPropertyException(contextBinding, propType, "resolver not found");
                    if (errorMode == ErrorHandlingMode.ReturnException)
                        return err;
                    ThrowException(err, contextBinding, propType);
                }
                return Value;
            }

            public static void ThrowException(Exception err, IBinding contextBinding, Type propType)
            {
                if (err is BindingPropertyException bpe && bpe.Binding == contextBinding)
                
                    throw bpe.Nest(propType);
                else
                    throw new BindingPropertyException(contextBinding, propType, err);
            }

            public PropValue<object> AsObject() => new PropValue<object>(Value, Error);


            public PropValue(TValue? value, Exception? error = null)
            {
                this.Value = value;
                this.Error = error;
            }
        }

        private protected struct MaybePropValue<TValue> where TValue : class
        {
            public PropValue<TValue> Value;
            public bool HasValue;

            public void SetValue(PropValue<TValue> r)
            {
                // we need to use Volatile, otherwise there is risk of setting HasValue before the actual error/value
                if (r.Error != null)
                {
                    // we can overwrite errors
                    // in case the Value already contains a value, this will not have any effect
                    Volatile.Write(ref Value.Error, r.Error);
                }
                else
                {
                    // value must not be overwritten
                    // if there was an error, we "overwrite" it, but that is not a problem
                    Interlocked.CompareExchange(ref Value.Value, r.Value, null);
                }
                Volatile.Write(ref HasValue, true);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            void ComputeValue(BindingExpression @this)
            {
                SetValue(@this.ComputeProperty<TValue>(null));
                Unsafe.Unbox
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PropValue<TValue> GetValue(BindingExpression @this)
            {
                if (!HasValue)
                    ComputeValue(@this);
                return Value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TValue? GetValueOrNull(BindingExpression @this) => GetValue(@this).Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TValue GetValueOrThrow(BindingExpression @this)
            {
                var x = GetValue(@this);
                if (x.Value is {})
                    return x.Value;
                ThrowError(x.Error, @this);
                return null!;
            }
            
            [MethodImpl(MethodImplOptions.NoInlining)]
            void ThrowError(Exception? exception, BindingExpression @this) =>
                PropValue<TValue>.ThrowException(exception ?? new BindingPropertyException(@this, typeof(TValue), "resolver not found"), @this, typeof(TValue));

        }

        // concurrencyLevel: 1, we don't need super high parallel performance, it better to save memory on all those locks
        private readonly ConcurrentDictionary<Type, PropValue<object>> properties = new(concurrencyLevel: 1, capacity: 8);
        protected readonly BindingCompilationService bindingService;

        // well, ConcurrentDictionary is pretty slow and takes a ton of memory, so we just do this for the most common properties
        private protected MaybePropValue<DataContextStack> dataContextStack;
        private protected MaybePropValue<BindingErrorReporterProperty> errorReporter;
        private protected MaybePropValue<BindingResolverCollection> resolverCollection;
        private protected MaybePropValue<ResolvedBinding> resolvedBinding;
        private protected MaybePropValue<KnockoutExpressionBindingProperty> knockoutExpressions;
        private protected MaybePropValue<ParsedExpressionBindingProperty> parsedExpression;
        private protected MaybePropValue<ResultTypeBindingProperty> resultType;
        private protected MaybePropValue<BindingParserOptions> parserOptions;
        private protected MaybePropValue<BindingDelegate> bindingDelegate;
        private protected MaybePropValue<AssignedPropertyBindingProperty> assignedProperty;
        private protected MaybePropValue<BindingCompilationRequirementsAttribute> compilationRequirements;
        private protected MaybePropValue<DotvvmLocationInfo> locationInfo;
        private protected MaybePropValue<BindingUpdateDelegate> updateDelegate;
        private protected MaybePropValue<OriginalStringBindingProperty> originalString;
        private protected MaybePropValue<ExpectedTypeBindingProperty> expectedType;

        public BindingExpression(BindingCompilationService service, IEnumerable<object?> properties)
        {
            this.toStringValue = new Lazy<string>(() => {
                // using Lazy improves performance a bit and most importantly handles StackOverflowException that could occur when OriginalStringBindingProperty getter fails
                string value;
                try
                {
                    value =
                        originalString.GetValueOrNull(this)?.Code ??
                        parsedExpression.GetValueOrNull(this)?.Expression?.ToString() ??
                        knockoutExpressions.GetValueOrNull(this)?.Code?.ToString(o => new Compilation.Javascript.CodeParameterAssignment($"${o.GetHashCode()}", Compilation.Javascript.OperatorPrecedence.Max)) ??
                        this.GetProperty<KnockoutJsExpressionBindingProperty>(ErrorHandlingMode.ReturnNull)?.Expression?.ToString() ??
                        "... unrepresentable binding content ...";
                }
                catch (Exception ex)
                {
                    // Binding.ToString is used in error handling, so it should not fail
                    value = $"Unable to get binding string due to {ex.GetType().Name}: {ex.Message}";
                }
                var typeName = this switch {
                    ControlPropertyBindingExpression => "controlProperty",
                    ValueBindingExpression => "value",
                    ResourceBindingExpression => "resource",
                    ControlCommandBindingExpression => "controlCommand",
                    StaticCommandBindingExpression => "staticCommand",
                    CommandBindingExpression => "command",
                    _ => this.GetType().Name
                };
                return $"{{{typeName}: {value}}}";
            });

            foreach (var prop in properties)
                if (prop != null) this.StoreProperty(prop);
            this.bindingService = service;
            service.InitializeBinding(this);
        }

        PropValue<object> GetPropertyNotInCache(Type propertyType)
        {
            // these really don't need to be stored in properties...
            // multiple different binding types (IBinding, IValueBinding, ValueBindingExpression) would be just polluting the concurrent dictionary
            if (propertyType == typeof(BindingCompilationService)) 
                return new(bindingService);
            if (propertyType.IsAssignableFrom(this.GetType()))
                return new(this);

            return StorePropertyInCache(propertyType, ComputeProperty<object>(propertyType));
        }

        PropValue<T> ComputeProperty<T>(Type? propertyType) where T : class
        {
            try
            {
                propertyType ??= typeof(T);
                var value = bindingService.ComputeProperty(propertyType, this);
                return value is Exception error ? new(null, error) : new((T?)value);
            }
            catch (Exception ex)
            {
                return new(null, ex);
            }
        }

        PropValue<object> StorePropertyInCache(Type propertyType, PropValue<object> r)
        {
            if (r.Error != null)
            {
                // overwrite previous error, this has a chance of being more descriptive (due to blocked recursion)
                return properties[propertyType] = r;
            }
            else
            {
                // don't overwrite value, it has to be singleton
                return properties.GetOrAdd(propertyType, r);
            }
        }

        void StoreProperty(object p)
        {
            if (p is DataContextStack dataContextStack)
                this.dataContextStack.SetValue(new(dataContextStack));
            else if (p is BindingErrorReporterProperty errorReporter)
                this.errorReporter.SetValue(new(errorReporter));
            else if (p is BindingResolverCollection resolverCollection)
                this.resolverCollection.SetValue(new(resolverCollection));
            else if (p is ResolvedBinding resolvedBinding)
                this.resolvedBinding.SetValue(new(resolvedBinding));
            else if (p is KnockoutExpressionBindingProperty knockoutExpressions)
                this.knockoutExpressions.SetValue(new(knockoutExpressions));
            else if (p is ParsedExpressionBindingProperty parsedExpression)
                this.parsedExpression.SetValue(new(parsedExpression));
            else if (p is ResultTypeBindingProperty resultType)
                this.resultType.SetValue(new(resultType));
            else if (p is BindingParserOptions parserOptions)
                this.parserOptions.SetValue(new(parserOptions));
            else if (p is BindingDelegate bindingDelegate)
                this.bindingDelegate.SetValue(new(bindingDelegate));
            else if (p is AssignedPropertyBindingProperty assignedProperty)
                this.assignedProperty.SetValue(new(assignedProperty));
            else if (p is BindingCompilationRequirementsAttribute compilationRequirements)
                this.compilationRequirements.SetValue(new(compilationRequirements));
            else if (p is DotvvmLocationInfo locationInfo)
                this.locationInfo.SetValue(new(locationInfo));
            else if (p is BindingUpdateDelegate updateDelegate)
                this.updateDelegate.SetValue(new(updateDelegate));
            else if (p is OriginalStringBindingProperty originalString)
                this.originalString.SetValue(new(originalString));
            else if (p is ExpectedTypeBindingProperty expectedType)
                this.expectedType.SetValue(new(expectedType));
            
            else
                StorePropertyInCache(p.GetType(), new(p));
        }

        private static Exception noResolversException = new Exception("There are no additional resolvers for this binding.");
        /// <summary>
        /// For performance reasons, derived bindings can set BindingResolverCollection to null to prevent runtime computation of the property which is somewhat costly
        /// </summary>
        protected void AddNullResolvers()
        {
            this.resolverCollection.SetValue(new(null, noResolversException));
        }

        public DataContextStack? DataContext => this.dataContextStack.GetValueOrNull(this);

        public object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException)
        {
            var propValue =
                type == typeof(DataContextStack) ? dataContextStack.GetValue(this).AsObject() :
                type == typeof(BindingErrorReporterProperty) ? errorReporter.GetValue(this).AsObject() :
                type == typeof(BindingResolverCollection) ? resolverCollection.GetValue(this).AsObject() :
                type == typeof(ResolvedBinding) ? resolvedBinding.GetValue(this).AsObject() :
                type == typeof(KnockoutExpressionBindingProperty) ? knockoutExpressions.GetValue(this).AsObject() :
                type == typeof(ParsedExpressionBindingProperty) ? parsedExpression.GetValue(this).AsObject() :
                type == typeof(ResultTypeBindingProperty) ? resultType.GetValue(this).AsObject() :
                type == typeof(BindingParserOptions) ? parserOptions.GetValue(this).AsObject() :
                type == typeof(BindingDelegate) ? bindingDelegate.GetValue(this).AsObject() :
                type == typeof(AssignedPropertyBindingProperty) ? assignedProperty.GetValue(this).AsObject() :
                type == typeof(BindingCompilationRequirementsAttribute) ? compilationRequirements.GetValue(this).AsObject() :
                type == typeof(DotvvmLocationInfo) ? locationInfo.GetValue(this).AsObject() :
                type == typeof(BindingUpdateDelegate) ? updateDelegate.GetValue(this).AsObject() :
                type == typeof(OriginalStringBindingProperty) ? originalString.GetValue(this).AsObject() :
                type == typeof(ExpectedTypeBindingProperty) ? expectedType.GetValue(this).AsObject() :

                properties.TryGetValue(type, out var result) ? result : GetPropertyNotInCache(type);

            return propValue.GetValue(errorMode, this, type);
        }



        Lazy<string> toStringValue;
        public override string ToString() => toStringValue.Value;

        IEnumerable<object> ICloneableBinding.GetAllComputedProperties()
        {
            return properties.Values.Select(p => p.Value).Concat(new object?[] {
                dataContextStack.GetValueOrNull(this),
                errorReporter.GetValueOrNull(this),
                resolverCollection.GetValueOrNull(this),
                resolvedBinding.GetValueOrNull(this),
                knockoutExpressions.GetValueOrNull(this),
                parsedExpression.GetValueOrNull(this),
                resultType.GetValueOrNull(this),
                parserOptions.GetValueOrNull(this),
                bindingDelegate.GetValueOrNull(this),
                assignedProperty.GetValueOrNull(this),
                compilationRequirements.GetValueOrNull(this),
                locationInfo.GetValueOrNull(this),
                updateDelegate.GetValueOrNull(this),
                originalString.GetValueOrNull(this),
                expectedType.GetValueOrNull(this),

            }).Where(p => p != null)!;
        }

        BindingResolverCollection? cachedAdditionalResolvers;

        public BindingResolverCollection? GetAdditionalResolvers()
        {
            if (cachedAdditionalResolvers is {})
                return cachedAdditionalResolvers;

            var prop = assignedProperty.Value.Value?.DotvvmProperty;
            var stack = dataContextStack.Value.Value;

            var attributes = prop?.GetAttributes<BindingCompilationOptionsAttribute>();
            var fromDataContext = stack?.GetAllBindingPropertyResolvers() ?? ImmutableArray<Delegate>.Empty;

            var resolvers = 
                fromDataContext.IsEmpty && attributes is null or { Length: 0 } ?
                    new BindingResolverCollection(Enumerable.Empty<Delegate>()) :
                    new BindingResolverCollection(
                        attributes
                        .SelectMany(o => o.GetResolvers())
                        .Concat(fromDataContext)
                        .ToArray());

            Interlocked.CompareExchange(ref cachedAdditionalResolvers, resolvers, null);
            return cachedAdditionalResolvers;
        }
    }
}
