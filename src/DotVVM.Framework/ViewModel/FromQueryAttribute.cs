using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// Binds the viewmodel property from the query string.
    /// </summary>
    public class FromQueryAttribute : ParameterBindingAttribute
    {

        public string ParameterName { get; }

        public FromQueryAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        protected override bool TryGetValueCore(IDotvvmRequestContext context, out object value)
        {
            var isPresent = context.Query.TryGetValue(ParameterName, out var stringValue);
            value = stringValue;
            return isPresent;
        }

    }
}