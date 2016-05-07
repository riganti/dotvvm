using DotVVM.Framework.Binding.Expressions;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    public class ClientIDFragment
    {
        public string Value { get; }
        public bool IsExpression { get; }

        public ClientIDFragment(string value, bool isExpression = false)
        {
            IsExpression = isExpression;
            Value = value;
        }

        public string ToJavascriptExpression()
            => IsExpression ? Value : JsonConvert.ToString(Value);

        public static ClientIDFragment FromProperty(object value)
        {
            if (value is IValueBinding) return new ClientIDFragment(((IValueBinding)value).GetKnockoutBindingExpression(), true);
            else return new ClientIDFragment((string)value);
        }
    }
}