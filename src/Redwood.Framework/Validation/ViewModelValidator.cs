using Redwood.Framework.ViewModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Validation
{
    public class ViewModelValidator
    {
        public ViewModelValidationProvider Provider { get; set; }
        public ViewModelValidator()
        {
            Provider = new ViewModelValidationProvider();
        }

        public bool ValidateByRules(IEnumerable<ValidationRule> rules, RedwoodValidationContext context)
        {
            var res = true;
            foreach (var rule in rules)
            {
                if (context.MatchGroups(rule.Groups))
                {
                    var value = rule.Property.GetValue(context.Value);
                    context.PushLevel(value, rule.PropertyName);

                    // validate the property
                    if (!rule.ValidationFunc(context))
                    {
                        if (!string.IsNullOrEmpty(rule.ErrorMessage))
                            context.AddError(rule.ErrorMessage);
                        res = false;
                    }

                    context.PopLevel();
                }
            }
            return res;
        }

        /// <summary>
        /// Validates the view model.
        /// </summary>
        public IEnumerable<ViewModelValidationError> ValidateViewModel(object viewModel, IEnumerable<string> groups = null)
        {
            var c = new RedwoodValidationContext(viewModel, groups)
            {
                Validator = this
            };
            ValidateViewModel(c);
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
            var rules = Provider.GetValidationRules(context.Value);
            return this.ValidateByRules(rules, context);
        }
    }
}
