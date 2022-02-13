using System;
using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Annotations;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    /// <summary>
    /// A base class for ComboBox form editor providers.
    /// </summary>
    public abstract class ComboBoxFormEditorProvider : FormEditorProviderBase
    {
        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return GetSettings(propertyInfo) != null;
        }

        public override DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            var comboBox = new ComboBox()
            {
                EmptyItemText = GetEmptyItemText(property, context)
            };

            comboBox.SetBinding(SelectorBase.ItemTextBindingProperty, context.CreateValueBinding(GetDisplayMember(property, context)));
            comboBox.SetBinding(SelectorBase.ItemValueBindingProperty, context.CreateValueBinding(GetValueMember(property, context)));

            comboBox.SetBinding(SelectorBase.ItemTextBindingProperty, context.CreateValueBinding(GetDisplayMember(property, context)));
            comboBox.SetBinding(SelectorBase.ItemValueBindingProperty, context.CreateValueBinding(GetValueMember(property, context)));
            comboBox.SetBinding(Selector.SelectedValueProperty, context.CreateValueBinding(property.PropertyInfo.Name));
            comboBox.SetBinding(ItemsControl.DataSourceProperty, GetDataSourceBinding(property, context, comboBox));

            var cssClass = ControlHelpers.ConcatCssClasses(ControlCssClass, property.Styles?.FormControlCssClass);
            if (!string.IsNullOrEmpty(cssClass))
            {
                comboBox.Attributes.Set("class", cssClass);
            }

            return comboBox;
        }

        /// <summary>
        /// Compiles the DataSource binding expression to a value binding.
        /// </summary>
        protected virtual ValueBindingExpression GetDataSourceBinding(PropertyDisplayMetadata property, DynamicDataContext context, ComboBox comboBox)
        {
            var dataSourceBindingExpression = GetDataSourceBindingExpression(property, context);
            if (string.IsNullOrEmpty(dataSourceBindingExpression))
            {
                throw new Exception($"The DataSource binding expression for property {property.PropertyInfo} must be specified!");
            }

            return context.CreateValueBinding(dataSourceBindingExpression);
        }

        /// <summary>
        /// Gets the DataSource binding expression for the ComboBox control from the ComboBoxSettingsAttribute.
        /// </summary>
        protected virtual string GetDataSourceBindingExpression(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return GetSettings(property.PropertyInfo)?.DataSourceBinding;
        }

        /// <summary>
        /// Gets the EmptyItemText for the ComboBox control from the ComboBoxSettingsAttribute.
        /// </summary>
        protected virtual string GetEmptyItemText(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return GetSettings(property.PropertyInfo)?.EmptyItemText;
        }

        /// <summary>
        /// Gets the ValueMember for the ComboBox control from the ComboBoxSettingsAttribute.
        /// </summary>
        protected virtual string GetValueMember(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return GetSettings(property.PropertyInfo)?.ValueMember;
        }

        /// <summary>
        /// Gets the DisplayMember for the ComboBox control from the ComboBoxSettingsAttribute.
        /// </summary>
        protected virtual string GetDisplayMember(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return GetSettings(property.PropertyInfo)?.DisplayMember;
        }

        private ComboBoxSettingsAttribute GetSettings(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<ComboBoxSettingsAttribute>();
        }
    }
}
