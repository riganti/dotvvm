#nullable enable

using System;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public class DotvvmPropertyAlias : DotvvmProperty
    {
        public DotvvmPropertyAlias(
            DotvvmProperty aliased,
            string alias)
        {
            Aliased = aliased;
            Name = alias;
            DeclaringType = aliased.DeclaringType;
            IsValueInherited = aliased.IsValueInherited;
            DefaultValue = aliased.DefaultValue;
        }

        public DotvvmProperty Aliased { get; }

        public override object? GetValue(DotvvmBindableObject control, bool inherit = true)
        {
            return Aliased.GetValue(control, inherit);
        }

        public override bool IsSet(DotvvmBindableObject control, bool inherit = true)
        {
            return Aliased.IsSet(control, inherit);
        }

        public override void SetValue(DotvvmBindableObject control, object? value)
        {
            Aliased.SetValue(control, value);
        }
    }
}
