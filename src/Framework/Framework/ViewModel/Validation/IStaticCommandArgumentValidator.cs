using System.Reflection;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel.Validation
{
    /// <summary> Validates the viewmodels in static command invocations. </summary>
    public interface IStaticCommandArgumentValidator
    {
        /// <summary> If validation errors are found, returns a ModelState listing them. This method is invoked by the framework before calling the static command method, if it has specified <c>[AllowStaticCommand(Validation = Automatic)]</c> </summary>
        StaticCommandModelState? ValidateStaticCommand(StaticCommandInvocationPlan staticCommandInvocation, object?[] arguments, IDotvvmRequestContext context);
    }
}
