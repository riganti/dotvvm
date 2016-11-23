using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelValidator : IViewModelValidator
    {
        private readonly IViewModelSerializationMapper viewModelSerializationMapper;

        public ViewModelValidator(IViewModelSerializationMapper viewModelMapper)
        {
            this.viewModelSerializationMapper = viewModelMapper;
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        public IEnumerable<ViewModelValidationError> ValidateViewModel(object viewModel)
        {
            return ValidateViewModel(viewModel, "", new HashSet<object>());
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        private IEnumerable<ViewModelValidationError> ValidateViewModel(object viewModel, string pathPrefix, HashSet<object> alreadyValidated)
        {
            if (alreadyValidated.Contains(viewModel)) yield break;

            if (viewModel == null)
            {
                yield break;
            }

            var viewModelType = viewModel.GetType();
            if (ViewModelJsonConverter.IsPrimitiveType(viewModelType) || ViewModelJsonConverter.IsNullableType(viewModelType))
            {
                yield break;
            }

            alreadyValidated.Add(viewModel);

            if (ViewModelJsonConverter.IsEnumerable(viewModelType))
            {
                // collections
                var index = 0;
                foreach (var item in (IEnumerable)viewModel)
                {
                    foreach (var error in ValidateViewModel(item, pathPrefix + "()[" + index + "]()", alreadyValidated))
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
                    var context = new ValidationContext(viewModel) { MemberName = property.Name };

                    foreach (var rule in property.ValidationRules)
                    {
                        var propertyResult = rule.SourceValidationAttribute?.GetValidationResult(value, context);
                        if (propertyResult != ValidationResult.Success)
                        {
                            yield return new ViewModelValidationError()
                            {
                                PropertyPath = path,
                                ErrorMessage = rule.ErrorMessage
                            };
                        }
                    }
                }

                // inspect objects
                if (value != null)
                {
                    if (ViewModelJsonConverter.IsComplexType(property.Type))
                    {
                        // complex objects
                        foreach (var error in ValidateViewModel(value, path, alreadyValidated))
                        {
                            yield return error;
                        }
                    }
                }
            }

            var validatableObjectViewModel = viewModel as IValidatableObject;
            if (validatableObjectViewModel != null)
            {
                foreach (var error in validatableObjectViewModel.Validate(new ValidationContext(viewModel)))
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
                        yield return new ViewModelValidationError()
                        {
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
