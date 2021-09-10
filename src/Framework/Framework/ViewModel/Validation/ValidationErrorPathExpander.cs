using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.ViewModel.Validation
{
    internal class ValidationErrorPathExpanderContext
    {
        internal readonly IDictionary<object, List<ViewModelValidationError>> ValidationErrorsLookup;
        internal readonly object? ValidationTarget;
        internal readonly ISet<object> AlreadyProcessedNodes;

        internal string? ValidationTargetPath { get; set; }

        public ValidationErrorPathExpanderContext(object? validationTarget, List<ViewModelValidationError> errors)
        {
            errors.ForEach(item => item.TargetObject = item.TargetObject ?? validationTarget);
            this.ValidationErrorsLookup = errors.GroupBy(e => e.TargetObject ?? validationTarget).ToDictionary(e => e.Key, e => e.ToList());
            this.ValidationTarget = validationTarget;
            this.AlreadyProcessedNodes = new HashSet<object>();
        }
    }

    internal class ValidationErrorPathExpander : IValidationErrorPathExpander
    {
        private readonly IViewModelSerializationMapper viewModelSerializationMapper;

        public ValidationErrorPathExpander(IViewModelSerializationMapper viewModelMapper)
        {
            this.viewModelSerializationMapper = viewModelMapper;
        }

        private void EnsurePropertyPathsAreCorrect(IEnumerable<ViewModelValidationError> errors)
        {
            if (errors.Any(error => error.PropertyPath != null && error.PropertyPath.Contains("()")))
            {
                var sb = new StringBuilder();
                sb.AppendLine("Knockout expressions are no longer supported in validation target paths.");
                sb.AppendLine("Validation target paths need to be rooted '/' and its segments delimited with the '/' character. Example: '/Property/InnerProperty'. ");
                sb.AppendLine($"Alternatively, consider using {nameof(ValidationErrorFactory)} since it generates these paths automatically in the correct form.");
                throw new ArgumentException(sb.ToString(), nameof(ViewModelValidationError.PropertyPath));
            }
        }

        public void Expand(ModelState modelState, object viewModel)
        {
            // Check that model state does not contain validation target paths in the old format
            EnsurePropertyPathsAreCorrect(modelState.Errors);

            if (modelState.Errors.All(e => IsPropertyPathRooted(e)))
            {
                // All validation target paths are already in the correct form
                // i.e. there is nothing to expand
                return;
            }

            // Add information about absolute paths to errors
            var modelStateDecoratorContext = new ValidationErrorPathExpanderContext(modelState.ValidationTarget, modelState.Errors);
            Expand(viewModel, string.Empty, modelStateDecoratorContext);

            // Remove not found errors
            modelState.Errors.RemoveAll(error => !modelStateDecoratorContext.AlreadyProcessedNodes.Contains(error.TargetObject));
        }

        private bool IsPropertyPathRooted(ViewModelValidationError error)
            => error.PropertyPath.StartsWith("/");

        private void Expand(object? viewModel, string pathPrefix, ValidationErrorPathExpanderContext context)
        {
            if (viewModel == null || context.AlreadyProcessedNodes.Contains(viewModel))
                return;

            if (viewModel == context.ValidationTarget)
                context.ValidationTargetPath = (pathPrefix != string.Empty) ? pathPrefix : "/";

            context.AlreadyProcessedNodes.Add(viewModel);
            var viewModelType = viewModel.GetType();

            if (ReflectionUtils.IsEnumerable(viewModelType))
            {
                // Traverse each element of a collection
                var index = 0;
                foreach (var item in (IEnumerable)viewModel)
                    Expand(item, $"{pathPrefix}/{index++}", context);

                return;
            }
            else
            {
                // Traverse all serializable properties
                var map = viewModelSerializationMapper.GetMap(viewModel.GetType());
                foreach (var property in map.Properties.Where(p => p.TransferToServer))
                {
                    var value = property.PropertyInfo.GetValue(viewModel);
                    if (value == null)
                        continue;

                    if (ReflectionUtils.IsComplexType(property.Type))
                        Expand(value, $"{pathPrefix}/{property.Name}", context);
                }
            }

            // Check if have assigned validation errors to this object
            if (context.ValidationErrorsLookup.TryGetValue(viewModel, out var validationErrors))
            {
                foreach (var validationError in validationErrors)
                {
                    var propertyName = validationError.PropertyPath ?? string.Empty;
                    if (propertyName.Length > 0 && propertyName[0] == '/')
                        continue;

                    var absolutePath = $"{pathPrefix}/{propertyName}".TrimEnd('/');
                    validationError.PropertyPath = (absolutePath != string.Empty) ? absolutePath : "/";
                }
            }
        }
    }
}
