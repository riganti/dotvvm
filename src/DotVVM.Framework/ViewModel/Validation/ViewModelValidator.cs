#nullable enable
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
        private readonly Dictionary<object, object> validationItems;

        public ViewModelValidator(IViewModelSerializationMapper viewModelMapper, DotvvmConfiguration dotvvmConfiguration)
        {
            this.viewModelSerializationMapper = viewModelMapper;
            this.validationItems = new Dictionary<object, object> { { typeof(DotvvmConfiguration), dotvvmConfiguration} };
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        public IEnumerable<ViewModelValidationError> ValidateViewModel(object? viewModel)
        {
            return ValidateViewModel(viewModel, "", new HashSet<object>());
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        private IEnumerable<ViewModelValidationError> ValidateViewModel(object? viewModel, string pathPrefix, HashSet<object> alreadyValidated)
        {
            if (viewModel == null)
            {
                yield break;
            }
            if (alreadyValidated.Contains(viewModel)) yield break;
            var viewModelType = viewModel.GetType();
            if (ReflectionUtils.IsPrimitiveType(viewModelType) || ReflectionUtils.IsNullableType(viewModelType))
            {
                yield break;
            }

            alreadyValidated.Add(viewModel);

            if (ReflectionUtils.IsEnumerable(viewModelType))
            {
                if (pathPrefix.Length == 0) pathPrefix = "$data";
                else pathPrefix += "()";

                // collections
                var index = 0;
                foreach (var item in (IEnumerable)viewModel)
                {
                    foreach (var error in ValidateViewModel(item, pathPrefix + "[" + index + "]()", alreadyValidated))
                    {
                        yield return error;
                    }
                    index++;
                }
                yield break;
            }

            // validate all properties on the object
            var map = viewModelSerializationMapper.GetMap(viewModel.GetType());
            foreach (var property in map.Properties.Where(p => p.TransferToServer))
            {
                var value = property.PropertyInfo.GetValue(viewModel);
                var path = CombinePath(pathPrefix, property.Name);

                // validate the property
                if (property.ValidationRules.Any())
                {
                    var context = new ValidationContext(viewModel, validationItems) { MemberName = property.Name };

                    foreach (var rule in property.ValidationRules)
                    {
                        var propertyResult = rule.SourceValidationAttribute?.GetValidationResult(value, context);
                        if (propertyResult != ValidationResult.Success)
                        {
                            yield return new ViewModelValidationError() {
                                PropertyPath = path,
                                ErrorMessage = rule.ErrorMessage
                            };
                        }
                    }
                }

                // inspect objects
                if (value != null)
                {
                    if (ReflectionUtils.IsComplexType(property.Type))
                    {
                        // complex objects
                        foreach (var error in ValidateViewModel(value, path, alreadyValidated))
                        {
                            yield return error;
                        }
                    }
                }
            }

            if (viewModel is IValidatableObject)
            {
                foreach (var error in ((IValidatableObject)viewModel).Validate(
                    new ValidationContext(viewModel, validationItems)))
                {
                    var paths = new List<string>();
                    if (error.MemberNames != null)
                    {
                        foreach (var memberName in error.MemberNames)
                        {
                            paths.Add(CombinePath(pathPrefix, memberName));
                        }
                    }
                    if (!paths.Any())
                    {
                        paths.Add(pathPrefix);
                    }

                    foreach (var memberPath in paths)
                    {
                        yield return new ViewModelValidationError() {
                            PropertyPath = memberPath,
                            ErrorMessage = error.ErrorMessage
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Combines the path.
        /// </summary>
        private string CombinePath(string prefix, string path)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return path;
            }
            else if (!prefix.EndsWith("()", StringComparison.Ordinal))
            {
                return prefix + "()." + path;
            }
            else
            {
                return prefix + "." + path;
            }
        }
    }
}
