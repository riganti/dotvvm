using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Controls.DynamicData.Configuration;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    /// <summary>
    /// Convention-based ComboBox form editor provider.
    /// </summary>
    public class ComboBoxConventionFormEditorProvider : ComboBoxFormEditorProvider
    {
        private readonly ComboBoxConventions comboBoxConventions;

        public ComboBoxConventionFormEditorProvider(ComboBoxConventions comboBoxConventions)
        {
            this.comboBoxConventions = comboBoxConventions;
        }

        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            var matchedConvention = comboBoxConventions.Conventions.FirstOrDefault(c => c.Match.IsMatch(propertyInfo));
            if (matchedConvention == null)
            {
                return false;
            }

            // store the convention in the context
            context.StateBag[new StateBagKey(this, propertyInfo)] = matchedConvention;
            return true;
        }

        protected override string GetDisplayMember(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return base.GetDisplayMember(property, context) ?? GetConvention(property, context).Settings.DisplayMember;
        }

        protected override string GetValueMember(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return base.GetValueMember(property, context) ?? GetConvention(property, context).Settings.ValueMember;
        }

        protected override string GetEmptyItemText(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return base.GetEmptyItemText(property, context) ?? GetConvention(property, context).Settings.EmptyItemText;
        }

        protected override string GetDataSourceBindingExpression(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return base.GetDataSourceBindingExpression(property, context) ?? GetConvention(property, context).Settings.DataSourceBinding;
        }

        private ComboBoxConvention GetConvention(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return (ComboBoxConvention)context.StateBag[new StateBagKey(this, property.PropertyInfo)];
        }
    }
}