using Redwood.Framework.ViewModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redwood.Framework.Validation
{
    public class ViewModelValidator
    {
        public ViewModelValidationProvider Provider { get; set; }

        public ViewModelValidator(ViewModelValidationProvider provider)
        {
            this.Provider = provider;
        }

        public bool ValidateByRules(IEnumerable<ValidationRule> rules, RedwoodValidationContext context)
        {
            var res = true;
            foreach (var rule in rules)
            {
                var value = rule.Property.GetValue(context.Value);
                context.PushLevel(value, rule.PropertyName);

                // validate the property even if shouldn't and report error only if should validate
                if (!rule.ValidationFunc(context) && context.ShouldValidate(rule))
                {
                    if (!string.IsNullOrEmpty(rule.ErrorMessage))
                        context.AddError(rule.ErrorMessage);
                    res = false;
                }

                context.PopLevel();
            }
            return res;
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        public IEnumerable<ViewModelValidationError> ValidateViewModel(object viewModel, IEnumerable<string> groups = null, string[] path = null)
        {
            var c = new RedwoodValidationContext(viewModel, groups)
            {
                Validator = this,
                ValidationPath = path ?? new string[0]
            };
            ValidateViewModel(c);
            return c.Errors;
        }

        public IEnumerable<ViewModelValidationError> ValidateViewModel(object viewModel, MethodInfo action, string[] path = null)
        {
            var rules = Provider.GetValidationRules(viewModel);
            IEnumerable<string> groups = Provider.ModifyRulesForAction(action, rules);
            var c = new RedwoodValidationContext(viewModel, groups)
            {
                Validator = this,
                ValidationPath = path ?? new string[0]
            };
            this.ValidateByRules(rules, c);
            return c.Errors;
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        public bool ValidateViewModel(RedwoodValidationContext context)
        {
            if (context.Value == null)
            {
                return true;
            }

            // validate all properties on the object
            var rules = context.Root == context.Value ?
                Provider.GetValidationRules(context.Value) :
                Provider.GetRulesForType(context.Value.GetType(), context.Root);

            return this.ValidateByRules(rules, context);
        }
    }
}
