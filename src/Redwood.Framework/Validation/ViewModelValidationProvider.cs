using Redwood.Framework.Runtime.Filters;
using Redwood.Framework.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Validation
{
    public class ViewModelValidationProvider
    {
        public List<IStaticViewModelValidationProvider> StaticProviders { get; private set; }
        public List<IStaticActionValidationProvider> ActionProviders { get; private set; }
        private readonly ConcurrentDictionary<Type, ValidationRule[]> CachedStaticRules = new ConcurrentDictionary<Type, ValidationRule[]>();

        public ViewModelValidationProvider(IEnumerable<IStaticViewModelValidationProvider> staticProviders, IEnumerable<IStaticActionValidationProvider> actionProviders)
        {
            this.StaticProviders = staticProviders.ToList();
            this.ActionProviders = actionProviders.ToList();
        }

        public static ViewModelValidationProvider CreateDefault()
        {
            return new ViewModelValidationProvider(
                new IStaticViewModelValidationProvider[] { new DataAnnotationsValidationProvider(), new TypeValidationProvider() },
                new [] { new AttributeActionValidationProvider() });
        }

        /// <summary>
        /// Gets validation rules for specified viewmodel instance, action executed
        /// </summary>
        public IEnumerable<ValidationRule> GetValidationRules(object viewModel)
        {
            IEnumerable<ValidationRule> r = CachedStaticRules.GetOrAdd(viewModel.GetType(), CreateStaticValidationRules);
            if(viewModel is IValidatingViewModel)
            {
                r = r.Concat(((IValidatingViewModel)viewModel).GetRules());
            }
            return r;
        }

        public IEnumerable<ValidationRule> GetRulesForType(Type type, object rootVm)
        {
            IEnumerable<ValidationRule> r = CachedStaticRules.GetOrAdd(type, CreateStaticValidationRules);
            if(rootVm is IValidatingViewModel)
            {
                r = r.Concat((rootVm as IValidatingViewModel).GetRulesFor(type));
            }
            return r;
        }

        /// <summary>
        /// Adds group condition to viewModelRules and returns groups for action call
        /// </summary>
        public HashSet<string> ModifyRulesForAction(MethodInfo mi, IEnumerable<ValidationRule> viewModelRules)
        {
            bool includeGlobal = true;
            HashSet<string> groups = new HashSet<string>();
            foreach (var p in ActionProviders)
            {
                bool ig = includeGlobal && p != ActionProviders[0];
                groups.UnionWith(p.ModifyRulesForAction(mi, viewModelRules, ref ig));
                includeGlobal = ig;
            }
            if (includeGlobal) groups.Add("*");
            return groups;
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
