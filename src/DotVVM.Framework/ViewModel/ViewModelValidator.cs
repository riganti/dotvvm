using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 

namespace DotVVM.Framework.ViewModel
{
    public class ViewModelValidator
    {
        /// <summary>
        /// Validates the view model.
        /// </summary>
        public IEnumerable<ViewModelValidationError> ValidateViewModel(object viewModel)
        {
            return ValidateViewModel(viewModel, "");
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        private IEnumerable<ViewModelValidationError> ValidateViewModel(object viewModel, string pathPrefix)
        {
            if (viewModel == null)
            {
                yield break;
            }
            var viewModelType = viewModel.GetType();
            if (ViewModelJsonConverter.IsPrimitiveType(viewModelType) || ViewModelJsonConverter.IsNullableType(viewModelType))
            {
                yield break;
            }

            // validate all properties on the object
            var map = ViewModelJsonConverter.GetSerializationMapForType(viewModel.GetType());
            foreach (var property in map.Properties)
            {
                var value = property.PropertyInfo.GetValue(viewModel);
                var path = CombinePath(pathPrefix, property.Name);

                // validate the property
                foreach (var rule in property.ValidationRules)
                {
                    if (!rule.SourceValidationAttribute.IsValid(value))
                    {
                        yield return new ViewModelValidationError()
                        {
                            PropertyPath = path,
                            ErrorMessage = rule.ErrorMessage
                        };
                    }
                }

                // inspect collections and complex objects
                if (value != null)
                {
                    if (ViewModelJsonConverter.IsEnumerable(property.Type))
                    {
                        // collections
                        var index = 0;
                        foreach (var item in (IEnumerable)value)
                        {
                            foreach (var error in ValidateViewModel(item, path + "()[" + index + "]"))
                            {
                                yield return error;
                            }
                            index++;
                        }
                    }
                    else if (ViewModelJsonConverter.IsComplexType(property.Type))
                    {
                        // complex objects
                        foreach (var error in ValidateViewModel(value, path))
                        {
                            yield return error;
                        }
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
            else if (!prefix.EndsWith("]"))
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
