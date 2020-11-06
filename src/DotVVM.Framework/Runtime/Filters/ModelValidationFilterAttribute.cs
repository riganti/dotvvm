#nullable enable
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Runs the model validation and returns the errors if the viewModel is not valid.
    /// </summary>
    public class ModelValidationFilterAttribute : ActionFilterAttribute
    {
        /// <inheritdoc />
        protected internal override Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            if (!string.IsNullOrEmpty(context.ModelState.ValidationTargetPath))
            {
                var validator = context.Services.GetRequiredService<IViewModelValidator>();
                context.ModelState.Errors.AddRange(validator.ValidateViewModel(context.ModelState.ValidationTarget, GetFullValidationPath(context)));
                context.FailOnInvalidModelState();
            }

            return TaskUtils.GetCompletedTask();
        }

        private string GetFullValidationPath(IDotvvmRequestContext context)
        {
            if (context.ModelState.ValidationTargetPath == "/")
                return context.ModelState.ValidationTargetPath;

            var data = context.ReceivedViewModelJson;
            if (data == null)
                return context.ModelState.ValidationTargetPath;

            var currentPathSegments = data["currentPath"].Values<string>();
            if (currentPathSegments.FirstOrDefault() == null)
                return context.ModelState.ValidationTargetPath;

            var currentPath = string.Join("/", data["currentPath"].Values<string>());
            var validationTargetPathPrefix = currentPath.Substring(currentPath.IndexOf(".") + 1);
            var validationTargetPath = context.ModelState.ValidationTargetPath;
            return $"{validationTargetPathPrefix}/{validationTargetPath}";
        }
    } 
}
