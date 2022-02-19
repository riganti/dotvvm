using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    public class EnumComboBoxFormEditorProvider : FormEditorProviderBase
    {
        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return propertyInfo.PropertyType.UnwrapNullableType().IsEnum;
        }

        public override DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicEditor.Props props, DynamicDataContext context)
        {
            var enumType = property.PropertyInfo.PropertyType.UnwrapNullableType();
            var isNullable = property.PropertyInfo.PropertyType.IsNullable();

            var options = Enum.GetNames(enumType)
                .Select(name => new
                {
                    Name = name,
                    DisplayName = enumType
                        .GetField(name)
                        ?.GetCustomAttribute<DisplayAttribute>()
                        ?.GetName() ?? name
                })
                .Select(e => new SelectorItem(e.DisplayName, Enum.Parse(enumType, e.Name)));

            var control = new ComboBox()
                .SetProperty(c => c.SelectedValue, (IValueBinding)context.CreateValueBinding(property));

            if (isNullable)
            {
                control.AppendChildren(new SelectorItem(property.NullDisplayText ?? "---", null!));
            }

            return control
                .AppendChildren(options);
        }
    }
}
