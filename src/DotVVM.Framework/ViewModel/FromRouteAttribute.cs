using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// Binds the viewmodel property from the route parameter.
    /// </summary>
    public class FromRouteAttribute : ParameterBindingAttribute
    {

        public string ParameterName { get; }

        public FromRouteAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        protected override bool TryGetValueCore(IDotvvmRequestContext context, out object? value)
        {
            return context.Parameters!.TryGetValue(ParameterName, out value);
        }
    }
}
