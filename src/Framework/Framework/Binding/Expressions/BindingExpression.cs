using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Binding.Expressions
{
    /// <summary> Represents a data-binding in DotVVM.
    /// This is a base class for all bindings, BindingExpression in general does not guarantee that the binding will have any property.
    /// This class only contains the glue code which automatically calls resolvers and caches the results when <see cref="GetProperty(Type, ErrorHandlingMode)" /> is invoked. </summary>
    [BindingCompilationRequirements(optional: new[] { typeof(BindingResolverCollection) })]
    [System.Text.Json.Serialization.JsonConverter(typeof(BindingDebugJsonConverter))]
    public abstract class BindingExpression : IBinding, ICloneableBinding
    {
        private protected readonly struct PropValue<TValue> where TValue : class
        {
            public readonly object UnsafeValue; // TValue or ErrorWrapper
            public ErrorWrapper? ErrorWrapper => UnsafeValue as ErrorWrapper;
            public Exception? Error => (UnsafeValue as ErrorWrapper)?.Error; // NOTE: might return if it's ResolverNotFoundSentinell

            [MemberNotNullWhen(false, "Value"), MemberNotNullWhen(true, "ErrorWrapper")]
            public bool HasError => UnsafeValue is ErrorWrapper;
            public TValue? Value => UnsafeValue is ErrorWrapper ? default : Unsafe.As<TValue>(UnsafeValue);

            public object? GetValue(ErrorHandlingMode errorMode, IBinding contextBinding, Type propType)
            {
                if (HasError)
                {
                    if (errorMode == ErrorHandlingMode.ReturnNull)
                        return null;
                    if (errorMode == ErrorHandlingMode.ReturnException)
                        return Error ?? GetError(contextBinding, propType);
                    return ThrowException(contextBinding, propType);
                }
                return Value;
            }

            public TValue GetOrThrow(IBinding contextBinding, Type? propType)
            {
                if (HasError) return ThrowException(contextBinding, propType);
                return Value!;
            }

            public Exception? GetError(IBinding contextBinding, Type? propType)
            {
                if (HasError)
                    return Error ?? new BindingPropertyException(contextBinding, propType ?? typeof(TValue), "resolver not found");
                return null;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public TValue ThrowException(IBinding contextBinding, Type? propType)
            {
                propType ??= typeof(TValue);
                var err = this.GetError(contextBinding, propType);
                if (err is null) throw new ArgumentException("this.Error");
                if (err is BindingPropertyException bpe && bpe.Binding == contextBinding)

                    throw bpe.Nest(propType);
                else
                    throw new BindingPropertyException(contextBinding, propType, err);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PropValue<object> AsObject() => PropValue<object>.FromUnsafeBoxed(this.UnsafeValue);

            public PropValue<T> UpCast<T>() where T : class
            {
                Debug.Assert(typeof(TValue).IsAssignableFrom(typeof(T)));
                if (!HasError)
                    _ = (T)UnsafeValue;
                return UpCastUnsafe<T>();
            }

            public PropValue<T> UpCastUnsafe<T>() where T : class // TODO: unnecessary
            {
                Debug.Assert(typeof(TValue).IsAssignableFrom(typeof(T)));
                Debug.Assert(UnsafeValue is BindingExpression.ErrorWrapper or T);
                return PropValue<T>.FromUnsafeBoxed(UnsafeValue);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Deconstruct(out TValue? value, out ErrorWrapper? error) => TryGet(out value, out error);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGet([NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out ErrorWrapper? error)
            {
                if (UnsafeValue is ErrorWrapper wrapper)
                {
                    error = wrapper;
                    value = null;
                    return false;
                }
                else
                {
                    error = null;
                    value = Unsafe.As<TValue>(UnsafeValue);
                    return true;
                }
            }

            internal PropValue(TValue? value, ErrorWrapper? error)
            {
                if (value is null && error is null)
                    throw new ArgumentException("Value or error must be set", nameof(error));
                this.UnsafeValue = (object?)error ?? value!;
            }

            private PropValue(object unsafeValue) { this.UnsafeValue = unsafeValue; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PropValue<TValue> FromUnsafeBoxed(object val) => new(val);
        }
        private protected interface IPropertyValueConverter<TValue, TProperty>
        {
            TValue ToValue(TProperty property);
            TProperty ToProperty(TValue value);
        }
        private protected struct MaybePropValue<TValue, TProperty, TConverter> where TValue : class where TConverter: struct
        {
            private object? value; // either TValue or ErrorWrapper
            public readonly bool IsComputed => value is not null;
            public readonly bool HasError => value is ErrorWrapper;
            public readonly object? UnsafeValue => value;
            public readonly TValue? Value => value is ErrorWrapper ? null : Unsafe.As<TValue>(value);
            public readonly Exception? Error => (value as ErrorWrapper)?.Error;

            [MemberNotNull(nameof(value))]
            public void SetValue(PropValue<TValue> r)
            {
                // make sure we don't overwrite an existing value, but we will try to overwrite an error (it might be more descriptive in case of blocked recursion)
                if (value is ErrorWrapper error)
                {
                    Interlocked.CompareExchange(ref value, r.UnsafeValue, comparand: error);
                }
                else
                {
                    Interlocked.CompareExchange(ref value, r.UnsafeValue, comparand: null);
                }
                Debug.Assert(value is ErrorWrapper or TValue);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            [MemberNotNull(nameof(value))]
            void ComputeValue(BindingExpression @this)
            {
                var newValue = @this.ComputeProperty(typeof(TValue));
                SetValue(PropValue<TValue>.FromUnsafeBoxed(newValue.UnsafeValue));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PropValue<TValue> GetValue(BindingExpression @this)
            {
                if (value is null)
                    ComputeValue(@this);
                return PropValue<TValue>.FromUnsafeBoxed(value);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TValue? GetValueOrNull(BindingExpression @this)
            {
                if (value is null)
                    ComputeValue(@this);
                return value is ErrorWrapper ? null : Unsafe.As<TValue>(value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TValue GetValueOrThrow(BindingExpression @this)
            {
                if (value is null)
                    ComputeValue(@this);
                return value is ErrorWrapper ? ThrowError(@this) : Unsafe.As<TValue>(value);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            TValue ThrowError(BindingExpression @this) =>
                PropValue<TValue>.FromUnsafeBoxed(value!).ThrowException(@this, typeof(TValue));
        }

        /// Optimization of MaybePropValue: Exceptions should be rare, so we wrap them in a sealed class to simplify the type check
        private protected sealed class ErrorWrapper(Exception? error)
        {
            public readonly Exception? Error = error;

            [return: NotNullIfNotNull(nameof(obj))]
            public static object? Unbox(object? obj) =>
                obj is ErrorWrapper wrapper ? wrapper.Error : obj;

            public static readonly ErrorWrapper ResolverNotFoundSentinell = new(null);
        }

        // concurrencyLevel: 1, we don't need super high parallel performance, it better to save memory on all those locks
        private ConcurrentDictionary<Type, PropValue<object>>? properties = null;
        [MemberNotNull("properties")]
        private void InitProperties()
        {
            if (properties is null)
                Interlocked.CompareExchange(ref properties, new(concurrencyLevel: 1, capacity: 1), null);
        }
        protected readonly BindingCompilationService bindingService;

        // well, ConcurrentDictionary is pretty slow and takes a ton of memory, so we just do this for the most common properties
        private protected MaybePropValue<DataContextStack> dataContextStack;
        private protected MaybePropValue<BindingErrorReporterProperty> errorReporter;
        private protected MaybePropValue<BindingResolverCollection> resolverCollection;
        private protected MaybePropValue<ResolvedBinding> resolvedBinding;
        private protected MaybePropValue<Expression> parsedExpression; // ParsedExpressionBindingProperty
        private protected MaybePropValue<Type> resultType; // ResultTypeBindingProperty
        private protected MaybePropValue<BindingParserOptions> parserOptions;
        private protected MaybePropValue<BindingDelegate> bindingDelegate;
        private protected MaybePropValue<DotvvmProperty> assignedProperty; // AssignedPropertyBindingProperty
        private protected MaybePropValue<BindingCompilationRequirementsAttribute> compilationRequirements;
        private protected MaybePropValue<DotvvmLocationInfo> locationInfo;
        private protected MaybePropValue<BindingUpdateDelegate, BindingUpdateDelegate, IdentityConverter> updateDelegate;
        private protected MaybePropValue<string, OriginalStringBindingProperty> originalString; // OriginalStringBindingProperty
        private protected MaybePropValue<Type, ExpectedTypeBindingProperty, > expectedType; // ExpectedTypeBindingProperty

        public BindingExpression(BindingCompilationService service, IEnumerable<object?> properties)
        {
            foreach (var prop in properties)
                if (prop != null) this.StoreProperty(prop);
            this.bindingService = service;
            service.InitializeBinding(this);
        }

        private protected ParsedExpressionBindingProperty GetParsedExpression(out ErrorWrapper? error) =>
            parsedExpression.GetValue(this).TryGet(out var value, out error)
                ? new ParsedExpressionBindingProperty(value)
                : default;

        private protected ResultTypeBindingProperty GetResultType(out ErrorWrapper? error) =>
            resultType.GetValue(this).TryGet(out var value, out error)
                ? new ResultTypeBindingProperty(value)
                : default;

        private protected AssignedPropertyBindingProperty GetAssignedProperty(out ErrorWrapper? error) =>
            assignedProperty.GetValue(this).TryGet(out var value, out error)
                ? new AssignedPropertyBindingProperty(value)
                : default;

        private protected OriginalStringBindingProperty GetOriginalString(out ErrorWrapper? error) =>
            originalString.GetValue(this).TryGet(out var value, out error)
                ? new OriginalStringBindingProperty(value)
                : default;

        private protected ExpectedTypeBindingProperty GetExpectedType(out ErrorWrapper? error) =>
            expectedType.GetValue(this).TryGet(out var value, out error)
                ? new ExpectedTypeBindingProperty(value)
                : default;

        PropValue<object> GetPropertyNotInCache(Type propertyType)
        {
            // these really don't need to be stored in properties...
            // multiple different binding types (IBinding, IValueBinding, ValueBindingExpression) would be just polluting the concurrent dictionary
            if (propertyType == typeof(BindingCompilationService))
                return new(bindingService, null);
            if (propertyType.IsAssignableFrom(this.GetType()))
                return new(this, null);

            return StorePropertyInCache(propertyType, ComputeProperty(propertyType));
        }

        PropValue<object> ComputeProperty(Type propertyType)
        {
            try
            {
                var value = bindingService.ComputeProperty(propertyType, this);
                return value switch
                {
                    null => new(null, ErrorWrapper.ResolverNotFoundSentinell),
                    Exception ex => new(null, new ErrorWrapper(ex)),
                    _ => new(value, null)
                };
            }
            catch (Exception ex)
            {
                return new(null, new ErrorWrapper(ex));
            }
        }

        PropValue<object> StorePropertyInCache(Type propertyType, PropValue<object> r)
        {
            InitProperties();
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

        private protected virtual void StoreProperty(object p)
        {
            if (p is DataContextStack dataContextStack)
                this.dataContextStack.SetValue(new(dataContextStack, null));
            else if (p is BindingErrorReporterProperty errorReporter)
                this.errorReporter.SetValue(new(errorReporter, null));
            else if (p is BindingResolverCollection resolverCollection)
                this.resolverCollection.SetValue(new(resolverCollection, null));
            else if (p is ResolvedBinding resolvedBinding)
                this.resolvedBinding.SetValue(new(resolvedBinding, null));
            else if (p is ParsedExpressionBindingProperty parsedExpression)
                this.parsedExpression.SetValue(new(parsedExpression.Expression, null));
            else if (p is ResultTypeBindingProperty resultType)
                this.resultType.SetValue(new(resultType.Type, null));
            else if (p is BindingParserOptions parserOptions)
                this.parserOptions.SetValue(new(parserOptions, null));
            else if (p is BindingDelegate bindingDelegate)
                this.bindingDelegate.SetValue(new(bindingDelegate, null));
            else if (p is AssignedPropertyBindingProperty assignedProperty)
                this.assignedProperty.SetValue(new(assignedProperty.DotvvmProperty, null));
            else if (p is BindingCompilationRequirementsAttribute compilationRequirements)
                this.compilationRequirements.SetValue(new(compilationRequirements, null));
            else if (p is DotvvmLocationInfo locationInfo)
                this.locationInfo.SetValue(new(locationInfo, null));
            else if (p is BindingUpdateDelegate updateDelegate)
                this.updateDelegate.SetValue(new(updateDelegate, null));
            else if (p is OriginalStringBindingProperty originalString)
                this.originalString.SetValue(new(originalString.Code, null));
            else if (p is ExpectedTypeBindingProperty expectedType)
                this.expectedType.SetValue(new(expectedType.Type, null));

            else
                StorePropertyInCache(p.GetType(), new(p, null));
        }

        private static ErrorWrapper noResolversException = new(new Exception("There are no additional resolvers for this binding."));
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
            PropValue<object> propValue = GetPropertyCore(type);
            return propValue.GetValue(errorMode, this, type);
        }

        public bool TryGetPropety<T>([NotNullWhen(true)] out T? value)
        {
            value = GetPropertyCore<T>(out var error);
            return error is null;
        }

        public bool TryGetPropety<T>([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out Exception? error)
        {
            value = GetPropertyCore<T>(out var errorWrapper)!;
            if (errorWrapper is { })
            {
                error = new PropValue<object>(null, errorWrapper).GetError(this, typeof(T))!;
                return false;
            }
            else
            {
                error = null;
                return true;
            }
        }

        public T GetProperty<T>()
        {
            var value = GetPropertyCore<T>(out var errorWrapper);
            if (errorWrapper is { })
            {
                new PropValue<object>(null, errorWrapper).ThrowException(this, typeof(T));
                return default!;
            }
            else
            {
                return value!;
            }
        }

        private protected PropValue<object> GetPropertyCore(Type type)
        {
            if (properties is { } && properties.TryGetValue(type, out var result)) return result;

            if (type.IsValueType)
            {
                ErrorWrapper? error;
                if (type == typeof(ParsedExpressionBindingProperty)) return new(GetParsedExpression(out error), error);
                if (type == typeof(ResultTypeBindingProperty)) return new(GetResultType(out error), error);
                if (type == typeof(AssignedPropertyBindingProperty)) return new(GetAssignedProperty(out error), error);
                if (type == typeof(OriginalStringBindingProperty)) return new(GetOriginalString(out error), error);
                if (type == typeof(ExpectedTypeBindingProperty)) return new(GetExpectedType(out error), error);
            }
            else
            {
                if (type == typeof(DataContextStack)) return dataContextStack.GetValue(this).AsObject();
                if (type == typeof(BindingErrorReporterProperty)) return errorReporter.GetValue(this).AsObject();
                if (type == typeof(BindingResolverCollection)) return resolverCollection.GetValue(this).AsObject();
                if (type == typeof(ResolvedBinding)) return resolvedBinding.GetValue(this).AsObject();
                if (type == typeof(BindingParserOptions)) return parserOptions.GetValue(this).AsObject();
                if (type == typeof(BindingDelegate)) return bindingDelegate.GetValue(this).AsObject();
                if (type == typeof(BindingCompilationRequirementsAttribute)) return compilationRequirements.GetValue(this).AsObject();
                if (type == typeof(DotvvmLocationInfo)) return locationInfo.GetValue(this).AsObject();
                if (type == typeof(BindingUpdateDelegate)) return updateDelegate.GetValue(this).AsObject();
            }

            if (TryGetPropertyVirtual(type, out var value)) return value;
            return GetPropertyNotInCache(type);
        }

        private protected T? GetPropertyCore<T>(out ErrorWrapper? error)
        {
            if (typeof(T).IsValueType)
            {
                if (typeof(T) == typeof(ParsedExpressionBindingProperty))
                    return (T)(object)GetParsedExpression(out error);
                if (typeof(T) == typeof(ResultTypeBindingProperty))
                    return (T)(object)GetResultType(out error);
                if (typeof(T) == typeof(AssignedPropertyBindingProperty))
                    return (T)(object)GetAssignedProperty(out error);
                if (typeof(T) == typeof(OriginalStringBindingProperty))
                    return (T)(object)GetOriginalString(out error);
                if (typeof(T) == typeof(ExpectedTypeBindingProperty))
                    return (T)(object)GetExpectedType(out error);
                if (TryGetPropertyVirtual<T>(out var value, out error))
                    return value;
            }
            else
            {
                // if (typeof(T) == typeof(DataContextStack)) dataContextStack.GetValue(this).Deconstruct(out Unsafe.As<T?, DataContextStack?>(ref value), out error);
                // else if (typeof(T) == typeof(BindingErrorReporterProperty)) errorReporter.GetValue(this).Deconstruct(out Unsafe.As<T?, BindingErrorReporterProperty?>(ref value), out error);
                // else if (typeof(T) == typeof(BindingResolverCollection)) resolverCollection.GetValue(this).Deconstruct(out Unsafe.As<T?, BindingResolverCollection?>(ref value), out error);
                // typeof(T) == typeof(ResolvedBinding) ? resolvedBinding.GetValue(this).UpCastUnsafe<T>() :
                // typeof(T) == typeof(BindingParserOptions) ? parserOptions.GetValue(this).UpCastUnsafe<T>() :
                // typeof(T) == typeof(BindingDelegate) ? bindingDelegate.GetValue(this).UpCastUnsafe<T>() :
                // typeof(T) == typeof(BindingCompilationRequirementsAttribute) ? compilationRequirements.GetValue(this).UpCastUnsafe<T>() :
                // typeof(T) == typeof(DotvvmLocationInfo) ? locationInfo.GetValue(this).UpCastUnsafe<T>() :
                // typeof(T) == typeof(BindingUpdateDelegate) ? updateDelegate.GetValue(this).UpCastUnsafe<T>() :

            }
            (var valueObj, error) = GetPropertyCore(typeof(T));
            return (T?)valueObj;
        }

        private protected virtual bool TryGetPropertyVirtual(Type type, out PropValue<object> value) { value = default; return false; }
        private protected virtual bool TryGetPropertyVirtual<T>(out T? value, out ErrorWrapper? error) { value = default; error = null; return false; }


        volatile string? toStringValue;
        public override string ToString()
        {
            if (toStringValue is null)
            {
                if (Monitor.IsEntered(this))
                {
                    // we are already executing ToString, to prevent stack overflow, return something else temporary
                    return "{binding ToString is being executed recursively}";
                }
                lock (this)
                {
                    toStringValue ??= CreateToString();
                }
            }
            return toStringValue;
        }

        string CreateToString()
        {
            string value;
            try
            {
                value =
                    originalString.GetValueOrNull(this) ??
                    parsedExpression.GetValueOrNull(this)?.ToString() ??
                    this.GetPropertyOrDefault<KnockoutExpressionBindingProperty>()?.Code?.ToString(o => new Compilation.Javascript.CodeParameterAssignment($"${o.GetHashCode()}", Compilation.Javascript.OperatorPrecedence.Max)) ??
                    this.GetPropertyOrDefault<KnockoutJsExpressionBindingProperty>().Expression?.ToString() ??
                    "... unrepresentable binding content ...";
            }
            catch (Exception ex)
            {
                // Binding.ToString is used in error handling, so it should not fail
                value = $"Unable to get binding string due to {ex.GetType().Name}: {ex.Message}";
            }
            var typeName = this.GetBindingName();
            return $"{{{typeName}: {value}}}";
        }

        IEnumerable<object> ICloneableBinding.GetAllComputedProperties()
        {
            return (properties?.Values.Select(p => p.Value) ?? Enumerable.Empty<object>()).Concat(GetOutOfDictionaryProperties()).Where(p => p != null)!;
        }

        private protected virtual IEnumerable<object?> GetOutOfDictionaryProperties()
        {
            return new object?[] {
                dataContextStack.Value,
                errorReporter.Value,
                resolverCollection.Value,
                resolvedBinding.Value,
                parsedExpression.Value,
                resultType.Value,
                parserOptions.Value,
                bindingDelegate.Value,
                assignedProperty.Value,
                compilationRequirements.Value,
                locationInfo.Value,
                updateDelegate.Value,
                originalString.Value,
                expectedType.Value,
            };
        }

        BindingResolverCollection? cachedAdditionalResolvers;

        public BindingResolverCollection? GetAdditionalResolvers()
        {
            if (cachedAdditionalResolvers is { })
                return cachedAdditionalResolvers;

            var prop = assignedProperty.Value;
            var stack = dataContextStack.Value;

            var attributes = prop?.GetAttributes<BindingCompilationOptionsAttribute>();
            var fromDataContext = stack?.GetAllBindingPropertyResolvers() ?? ImmutableArray<Delegate>.Empty;

            var resolvers =
                fromDataContext.IsEmpty && attributes is null or { Length: 0 } ?
                    new BindingResolverCollection(Enumerable.Empty<Delegate>()) :
                    new BindingResolverCollection(
                        (attributes?.SelectMany(o => o.GetResolvers()) ?? Enumerable.Empty<Delegate>())
                        .Concat(fromDataContext)
                        .ToArray());

            Interlocked.CompareExchange(ref cachedAdditionalResolvers, resolvers, null);
            return cachedAdditionalResolvers;
        }
    }
}
