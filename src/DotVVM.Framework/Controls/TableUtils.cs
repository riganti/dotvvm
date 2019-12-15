#nullable enable
using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class TableUtils
    {
        /// <summary>
        /// Hides entire column in the table. Should be applied to the header.
        /// Does not check for correct usage, may give JS errors, check out the console if it does not work.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public static readonly DotvvmProperty ColumnVisibleProperty
            = DelegateActionProperty<bool>.Register<TableUtils>("ColumnVisible", (writer, context, property, control) =>
            {
                var binding = control.GetValueBinding(property);
                if (binding != null)
                    writer.AddKnockoutDataBind("dotvvm-table-columnvisible", binding.GetKnockoutBindingExpression(control));
            }, defaultValue: true);

    }
}
