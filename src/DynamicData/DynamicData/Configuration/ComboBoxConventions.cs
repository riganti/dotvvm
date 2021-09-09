using System;
using System.Collections.Generic;
using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Annotations;
using DotVVM.Framework.Controls.DynamicData.Utils;

namespace DotVVM.Framework.Controls.DynamicData.Configuration
{
    public class ComboBoxConventions
    {

        public List<ComboBoxConvention> Conventions { get; } = new List<ComboBoxConvention>();
        

        public void Register(string propertyName, ComboBoxSettingsAttribute settings)
        {
            Conventions.Add(new ComboBoxConvention()
            {
                Match = new PropertyNameMatch(propertyName),
                Settings = settings
            });
        }

        public void Register(PropertyInfo property, ComboBoxSettingsAttribute settings)
        {
            Conventions.Add(new ComboBoxConvention()
            {
                Match = new PropertyExactMatch(property),
                Settings = settings
            });
        }

        public void Register(Func<PropertyInfo, bool> predicate, ComboBoxSettingsAttribute settings)
        {
            Conventions.Add(new ComboBoxConvention()
            {
                Match = new PropertyDelegateMatch() { Delegate = predicate },
                Settings = settings
            });
        }

    }
}