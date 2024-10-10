using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelValidator : IViewModelValidator
    {
        private readonly IViewModelSerializationMapper viewModelSerializationMapper;
        private readonly Dictionary<object, object?> validationItems;

        public ViewModelValidator(IViewModelSerializationMapper viewModelMapper, DotvvmConfiguration dotvvmConfiguration)
        {
            this.viewModelSerializationMapper = viewModelMapper;
            this.validationItems = new Dictionary<object, object?> { { typeof(DotvvmConfiguration), dotvvmConfiguration} };
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        public IEnumerable<ViewModelValidationError> ValidateViewModel(object? viewModel)
        {
            return ValidateViewModel(viewModel, null, viewModel, "");
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        private IEnumerable<ViewModelValidationError> ValidateViewModel(object? viewModel, HashSet<object>? alreadyValidated, object? rootObject, string pathPrefix)
        {
            alreadyValidated ??= new HashSet<object>();
            if (!(pathPrefix.Length == 0 || pathPrefix.EndsWith("/")))
                throw new Exception("Invalid path!");

            if (viewModel == null)
            {
                yield break;
            }
            if (alreadyValidated.Contains(viewModel)) yield break;
            var viewModelType = viewModel.GetType();
            if (ReflectionUtils.IsDotvvmNativePrimitiveType(viewModelType))
            {
                yield break;
            }
            alreadyValidated.Add(viewModel);

            if (ReflectionUtils.IsEnumerable(viewModelType))
            {
                // collections
                var index = 0;
                foreach (var item in (IEnumerable)viewModel)
                {
                    foreach (var error in ValidateViewModel(item, alreadyValidated, rootObject, pathPrefix + index + "/"))
                    {
                        yield return error;
                    }
                    index++;
                }
                yield break;
            }

            // validate all properties on the object
            var map = viewModelSerializationMapper.GetMap(viewModel.GetType());
            var dotvvmConfiguration = (DotvvmConfiguration)validationItems[typeof(DotvvmConfiguration)]!;

            foreach (var property in map.Properties.Where(p => p.TransferToServer))
            {
                var value = property.PropertyInfo.GetValue(viewModel);

                // validate the property
                if (property.ValidationRules.Any())
                {
                    var context = new ValidationContext(viewModel, validationItems) { MemberName = property.Name };
                    context.InitializeServiceProvider(dotvvmConfiguration.ServiceProvider.GetService);

                    foreach (var rule in property.ValidationRules)
                    {
                        var propertyResult = rule.SourceValidationAttribute?.GetValidationResult(value, context);
                        if (propertyResult != ValidationResult.Success)
                        {
                            var propertyPath =
                                viewModel is IDotvvmPrimitiveType ? pathPrefix.TrimEnd('/') : pathPrefix + property.Name;
                            yield return new ViewModelValidationError(rule.ErrorMessage, propertyPath, rootObject);
                        }
                    }
                }

                // inspect objects
                if (value != null)
                {
                    if (!ReflectionUtils.IsDotvvmNativePrimitiveType(property.Type))
                    {
                        // complex objects
                        foreach (var error in ValidateViewModel(value, alreadyValidated, rootObject, pathPrefix + property.Name + "/"))
                        {
                            yield return error;
                        }
                    }
                }
            }

            if (viewModel is IValidatableObject)
            {
                var validationContext = new ValidationContext(viewModel, validationItems);
                validationContext.InitializeServiceProvider(dotvvmConfiguration.ServiceProvider.GetService);
                var errors = ((IValidatableObject)viewModel).Validate(validationContext);

                foreach (var error in errors)
                {
                    var paths = new List<string>();
                    if (error.MemberNames != null)
                    {
                        foreach (var memberPath in error.MemberNames)
                        {
                            paths.Add(memberPath);
                        }
                    }

                    if (!paths.Any())
                    {
                        paths.Add(string.Empty);
                    }

                    foreach (var memberPath in paths)
                    {
                        yield return new ViewModelValidationError(error.ErrorMessage ?? "An unknown error.", (pathPrefix + memberPath).TrimEnd('/'), rootObject);
                    }
                }
            }
        }
    }
}
