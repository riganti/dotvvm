using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq; 

namespace DotVVM.Framework.ViewModel
{
    public class ViewModelValidator
    {
        /// <summary>
        /// Validates the view model.
        /// </summary>
        public IEnumerable<ViewModelValidationError> ValidateViewModel(object viewModel, ISet<string> groups)
        {
            return ValidateViewModel(viewModel, "", groups ?? new HashSet<string>(), new HashSet<object>());
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        private IEnumerable<ViewModelValidationError> ValidateViewModel(object viewModel, string pathPrefix, ISet<string> groups, HashSet<object> alreadyValidated)
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
                    foreach (var error in ValidateViewModel(item, pathPrefix + "()[" + index + "]", groups, alreadyValidated))
                    {
                        yield return error;
                    }
                    index++;
                }
                yield break;
            }

            // validate all properties on the object
            var map = ViewModelJsonConverter.GetSerializationMapForType(viewModel.GetType());
            foreach (var property in map.Properties.Where(p => p.TransferToServer))
            {
                if(property.ValidationSettings != null)
                {
                    if (property.ValidationSettings.OnlyInGroups != null && !property.ValidationSettings.OnlyInGroups.Any(groups.Contains)) continue;
                    if (property.ValidationSettings.NotInGroups != null && property.ValidationSettings.NotInGroups.Any(groups.Contains)) continue;
                }

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
                    if (ViewModelJsonConverter.IsComplexType(property.Type) && (property.ValidationSettings == null || !property.ValidationSettings.OnlyInTarget))
                    {
                        // complex objects
                        foreach (var error in ValidateViewModel(value, path, groups, alreadyValidated))
                        {
                            yield return error;
                        }
                    }
                }
            }

            if (viewModel is IValidatableObject)
            {
                foreach (var error in ((IValidatableObject)viewModel).Validate(new ValidationContext(viewModel)))
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
            else if (!prefix.EndsWith("]", StringComparison.Ordinal))
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
