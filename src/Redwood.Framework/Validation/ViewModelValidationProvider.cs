using Redwood.Framework.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Validation
{
    public class ViewModelValidationProvider
    {
        public readonly List<IStaticViewModelValidationProvider> StaticProviders = new List<IStaticViewModelValidationProvider>();
        private readonly ConcurrentDictionary<Type, ValidationRule[]> CachedStaticRules = new ConcurrentDictionary<Type, ValidationRule[]>();

        public ViewModelValidationProvider(IStaticViewModelValidationProvider[] staticProviders = null)
        {
            if (staticProviders == null)
            {
                this.StaticProviders.Add(new DataAnnotationsValidationProvider());
                this.StaticProviders.Add(new TypeValidationProvider());
            }
            else this.StaticProviders.AddRange(staticProviders);
        }

        /// <summary>
        /// Gets validation rules for specified viewmodel instance, action executed
        /// </summary>
        public ValidationRule[] GetValidationRules(object viewModel)
        {
            var stat = CachedStaticRules.GetOrAdd(viewModel.GetType(), CreateStaticValidationRules);
            // TODO: include dynamic rules from viewModel and rules for action validation
            return stat;
        }

        public ValidationRule[] GetRulesForType(Type type, object rootVm)
        {
            return CachedStaticRules.GetOrAdd(type, CreateStaticValidationRules);
            // TODO: include dynamic rules from viewModel and rules for action validation
        }

        protected virtual ValidationRule[] CreateStaticValidationRules(Type viewModel)
        {
            var propeties = ViewModelJsonConverter.GetSerializationMapForType(viewModel).Properties
                .Where(p => p.TransferToServer && p.ViewModelProtection == ViewModelProtectionSettings.None).ToArray();
            var rules = new List<ValidationRule>();
            foreach (var prov in StaticProviders)
            {
                rules.AddRange(prov.GetGlobalRules(viewModel));
                foreach (var prop in propeties)
                {
                    rules.AddRange(prov.GetRules(prop.PropertyInfo).Select(PropertyInfoIncuder(prop)));
                }
            }
            return rules.ToArray();
        }

        protected Func<ValidationRule, ValidationRule> PropertyInfoIncuder(ViewModelPropertyMap map)
        {
            return rule =>
            {
                if (rule.PropertyName == null) rule.PropertyName = map.Name;
                if (rule.Property == null) rule.Property = map.PropertyInfo;
                return rule;
            };
        }
    }
}
