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
        internal readonly IDictionary<object, int> FoundErrors;

        internal string? ValidationTargetPath { get; set; }

        public ValidationErrorPathExpanderContext(object? validationTarget, List<ViewModelValidationError> errors)
        {
            errors.ForEach(item => item.TargetObject ??= validationTarget);
            this.ValidationErrorsLookup = errors.GroupBy(e => e.TargetObject ?? validationTarget!).ToDictionary(e => e.Key, e => e.ToList());
            this.ValidationTarget = validationTarget;
            this.AlreadyProcessedNodes = new HashSet<object>();
            this.FoundErrors = new Dictionary<object, int>();
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

        public void Expand(ModelState modelState, object? viewModel)
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
            var modelStateDecoratorContext = new ValidationErrorPathExpanderContext(modelState.ValidationTarget, modelState.ErrorsInternal);
            Expand(viewModel, string.Empty, modelStateDecoratorContext);

            // Remove not found errors
            modelState.ErrorsInternal.RemoveAll(error => error.TargetObject != null && !modelStateDecoratorContext.AlreadyProcessedNodes.Contains(error.TargetObject));
        }

        private bool IsPropertyPathRooted(ViewModelValidationError error)
            => error.PropertyPath != null && error.PropertyPath.StartsWith("/");

        private int Expand(object? viewModel, string pathPrefix, ValidationErrorPathExpanderContext context)
        {
            var errorsCount = 0;
            if (viewModel == null)
                return errorsCount;

            if (context.AlreadyProcessedNodes.Contains(viewModel))
                EnsureNoErrorsAttachedOnViewModel(viewModel, context);

            if (viewModel == context.ValidationTarget)
                context.ValidationTargetPath = (pathPrefix != string.Empty) ? pathPrefix : "/";

            context.AlreadyProcessedNodes.Add(viewModel);
            var viewModelType = viewModel.GetType();

            if (ReflectionUtils.IsEnumerable(viewModelType))
            {
                // Traverse each element of a collection
                var index = 0;
                foreach (var item in (IEnumerable)viewModel)
                {
                    var innerErrorsCount = Expand(item, $"{pathPrefix}/{index++}", context);
                    context.FoundErrors[item] = innerErrorsCount;
                    errorsCount += innerErrorsCount;
                }

                context.FoundErrors[viewModel] = errorsCount;
                return errorsCount;
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
                    {
                        var innerErrorsCount = Expand(value, $"{pathPrefix}/{property.Name}", context);
                        context.FoundErrors[value] = innerErrorsCount;
                        errorsCount += innerErrorsCount;
                    }

                    context.FoundErrors[viewModel] = errorsCount;
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
                    errorsCount++;
                }
            }

            return errorsCount;
        }

        private void EnsureNoErrorsAttachedOnViewModel(object viewModel, ValidationErrorPathExpanderContext context)
        {
            if (context.FoundErrors.ContainsKey(viewModel) && context.FoundErrors[viewModel] > 0)
            {
                throw new InvalidOperationException($"Could not generate path for a validation error. " +
                    $"An object with one or more errors is referenced multiple times in a viewmodel.");
            }
        }
    }
}
