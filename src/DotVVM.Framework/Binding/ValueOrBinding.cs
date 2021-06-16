#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using Newtonsoft.Json;

namespace DotVVM.Framework.Binding
{
    public interface ValueOrBinding
    {
        IBinding? BindingOrDefault { get; }
        object? BoxedValue { get; }
    }

    public struct ValueOrBinding<T> : ValueOrBinding
    {
        private readonly IBinding? binding;
        [AllowNull]
        private readonly T value;

        private ValueOrBinding(IBinding? binding, [AllowNull] T value)
        {
            this.binding = binding;
            this.value = value;
        }

        public ValueOrBinding(IBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (binding.GetProperty<ResultTypeBindingProperty>(ErrorHandlingMode.ReturnNull) is ResultTypeBindingProperty resultType &&
                    !typeof(T).IsAssignableFrom(resultType.Type))
                throw new ArgumentException($"The binding result type {resultType.Type.FullName} is not assignable to {typeof(T).FullName}");
            this.binding = binding;
            this.value = default;
        }

        public ValueOrBinding(IStaticValueBinding<T> binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            // result type check is unnecessary when binding is generic
            this.binding = binding;
            this.value = default;
        }

        public ValueOrBinding(T value)
        {
            this.value = value;
            this.binding = default;
        }

        public static ValueOrBinding<T> FromBoxedValue(object? value) =>
            value is IBinding binding ? new ValueOrBinding<T>(binding) :
            value is ValueOrBinding vob ? new ValueOrBinding<T>(vob.BindingOrDefault, (T)vob.BoxedValue!) :
            new ValueOrBinding<T>((T)value!);


        public T Evaluate(DotvvmBindableObject control) =>
            binding != null ? (T)binding.GetBindingValue(control) : value;

        public T ValueOrDefault => value;
        public IBinding? BindingOrDefault => binding;
        public object? BoxedValue => (object?)value;

        public static ValueOrBinding<T> DownCast<T2>(ValueOrBinding<T2> createFrom)
            where T2 : T => new ValueOrBinding<T>(createFrom.binding, createFrom.value!);


        public ValueOrBinding<T2> UpCast<T2>()
            where T2 : T =>
            this.binding != null ?
            new ValueOrBinding<T2>(this.binding) :
            new ValueOrBinding<T2>((T2)this.value!);

        public ParametrizedCode GetParametrizedJsExpression(DotvvmBindableObject control, bool unwrapped = false) =>
            ProcessValueBinding(control,
                value => new ParametrizedCode(JsonConvert.SerializeObject(value), OperatorPrecedence.Max),
                binding => binding.GetParametrizedKnockoutExpression(control, unwrapped)
            );

        public string GetJsExpression(DotvvmBindableObject control, bool unwrapped = false) =>
            ProcessValueBinding(control,
                value => JsonConvert.SerializeObject(value),
                binding => binding.GetKnockoutBindingExpression(control, unwrapped)
            );


        // TODO: proper mapping operators

        // public ValueOrBinding<TNew> Map<TNew>(Func<T, TNew> valueMap, Func<IBinding, IBinding>? bindingMap = null) =>
        //     binding != null ?
        //     new ValueOrBinding<TNew>(bindingMap == null ? binding : bindingMap.Invoke(binding)) :
        //     new ValueOrBinding<TNew>(valueMap(value));

        // public ValueOrBinding<TNew> Bind<TNew>(Func<T, ValueOrBinding<TNew>> valueMap, Func<IBinding, ValueOrBinding<TNew>>? bindingMap = null) =>
        //     binding != null ?
        //     (bindingMap == null ? new ValueOrBinding<TNew>(binding) : bindingMap.Invoke(binding)) :
        //     valueMap(value);

        public void Process(Action<T> processValue, Action<IBinding> processBinding)
        {
            if (binding != null)
                processBinding?.Invoke(binding);
            else processValue?.Invoke(value);
        }

        public TResult Process<TResult>(Func<T, TResult> processValue, Func<IBinding, TResult> processBinding)
        {
            if (binding != null)
                return processBinding(binding);
            else return processValue(value);
        }

        public void ProcessValueBinding(DotvvmBindableObject control, Action<T> processValue, Action<IValueBinding> processBinding)
        {
            if (binding == null)
                processValue?.Invoke(value);
            else if (binding is IValueBinding valueBinding)
                processBinding?.Invoke(valueBinding);
            else
                processValue?.Invoke(this.Evaluate(control));
        }

        public TResult ProcessValueBinding<TResult>(DotvvmBindableObject control, Func<T, TResult> processValue, Func<IValueBinding, TResult> processBinding)
        {
            if (binding == null)
                return processValue(value);
            else if (binding is IValueBinding valueBinding)
                return processBinding(valueBinding);
            else
                return processValue(this.Evaluate(control));
        }

        public static implicit operator ValueOrBinding<T>(T val) => new ValueOrBinding<T>(val);

        public const string EqualsDisabledReason = "Equals is disabled on ValueOrBinding<T> as it may lead to unexpected behavior. Please use object.ReferenceEquals for reference comparison or evalate the ValueOrBinding<T> and compare the value.";
        [Obsolete(EqualsDisabledReason, error: true)]
        public static bool operator ==(ValueOrBinding<T> a, ValueOrBinding<T> b) =>
            throw new NotSupportedException(EqualsDisabledReason);
        [Obsolete(EqualsDisabledReason, error: true)]
        public static bool operator !=(ValueOrBinding<T> a, ValueOrBinding<T> b) =>
            throw new NotSupportedException(EqualsDisabledReason);

        [Obsolete(EqualsDisabledReason, error: true)]
        public override bool Equals(object? obj) => throw new NotSupportedException(EqualsDisabledReason);

        [Obsolete(EqualsDisabledReason, error: true)]
        public override int GetHashCode() => throw new NotSupportedException(EqualsDisabledReason);
    }
}
