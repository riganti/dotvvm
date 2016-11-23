using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement.ClientGlobalize;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelValidationRuleTranslator : IValidationRuleTranslator
    {
        private readonly DotvvmConfiguration dotvvmConfiguration;

        public ViewModelValidationRuleTranslator(DotvvmConfiguration dotvvmConfiguration)
        {
            this.dotvvmConfiguration = dotvvmConfiguration;
        }

        /// <summary>
        /// Gets the validation rules.
        /// </summary>
        public virtual IEnumerable<IViewModelPropertyValidationRule> TranslateValidationRules(PropertyInfo property, IEnumerable<ValidationAttribute> validationAttributes)
        {
            foreach (var attribute in validationAttributes)
            {
                var clientValidationFactory = dotvvmConfiguration.ValidationConfiguration.GetClientValidationRule(attribute.GetType());
                yield return clientValidationFactory.CreateViewModelPropertyValidationRule(attribute, property.Name);
            }
        }
    }
}