using System;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using Newtonsoft.Json;

namespace DotVVM.Framework.Binding
{
    public abstract class ValueOrBinding
    {
        public abstract IBinding BindingOrDefault { get; }
        public abstract object BoxedValue { get; }
    }

    public class ValueOrBinding<T> : ValueOrBinding
    {
        private readonly IBinding binding;
        private readonly T value;

        public ValueOrBinding(IBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (binding.GetProperty<ResultTypeBindingProperty>(ErrorHandlingMode.ReturnNull) is ResultTypeBindingProperty resultType &&
                    !typeof(T).IsAssignableFrom(resultType.Type))
                    throw new ArgumentException($"The binding result type {resultType.Type.FullName} is not assignable to {typeof(T).FullName}");
            this.binding = binding;
        }

        public ValueOrBinding(T value)
        {
            this.value = value;
        }

        public T Evaluate(DotvvmBindableObject control) =>
            binding != null ? (T)binding.GetBindingValue(control) : value;

        public T ValueOrDefault => value;
        public override IBinding BindingOrDefault => binding;
        public override object BoxedValue => (object)value;

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

        public ValueOrBinding<TNew> Map<TNew>(Func<T, TNew> valueMap, Func<IBinding, IBinding> bindingMap = null) =>
            binding != null ?
            new ValueOrBinding<TNew>(bindingMap == null ? binding : bindingMap.Invoke(binding)) :
            new ValueOrBinding<TNew>(valueMap(value));

        public ValueOrBinding<TNew> Bind<TNew>(Func<T, ValueOrBinding<TNew>> valueMap, Func<IBinding, ValueOrBinding<TNew>> bindingMap = null) =>
            binding != null ?
            (bindingMap == null ? new ValueOrBinding<TNew>(binding) : bindingMap.Invoke(binding)) :
            valueMap(value);

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
    }
}