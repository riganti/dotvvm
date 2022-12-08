using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the HTML list box - <c>&lt;select size="10"</c> element.
    /// This component allows selecting only one value, for multiple selection use the <see cref="MultiSelect"/> component.
    /// </summary>
    public class ListBox : SelectHtmlControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        public ListBox()
        {

        }

        /// <summary>
        /// Gets or sets number of rows visible in this ListBox.
        /// </summary>
        public int Size
        {
            get { return (int)GetValue(SizeProperty)!; }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DotvvmProperty SizeProperty =
            DotvvmProperty.Register<int, ListBox>(t => t.Size, defaultValue: 10);

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.AddAttributesToRender(writer, context);
            writer.AddKnockoutDataBind("size", this, SizeProperty, () => writer.AddAttribute("size", Size.ToString()));
        }

        [ControlUsageValidator(Override = true)]
        public static new IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if (!(control.GetValue(SelectedValueProperty) is ResolvedPropertySetter selectedValueBinding)) yield break;

            if (control.GetValue(ItemValueBindingProperty) is ResolvedPropertySetter itemValueBinding)
            {
                var to = selectedValueBinding.GetResultType();
                var from = itemValueBinding.GetResultType();

                if (!IsValueAssignable(from, to))
                {
                    yield return CreateSelectedValueTypeError(selectedValueBinding, to, from);
                }
            }
            else if (control.GetValue(DataSourceProperty) is ResolvedPropertySetter dataSourceBinding)
            {
                var to = selectedValueBinding.GetResultType();
                var from = dataSourceBinding.GetResultType()?.UnwrapNullableType()?.GetEnumerableType();

                if (!IsCollectionPropertySetter(dataSourceBinding))
                {
                    yield return new ($"{nameof(DataSource)} must be a collection.", selectedValueBinding.DothtmlNode);
                }
                if (!IsDataSourceItemAssignable(from, to))
                {
                    yield return CreateSelectedValueTypeError(selectedValueBinding, to, from);
                }
            }
        }

        private static bool IsCollectionPropertySetter(ResolvedPropertySetter setter)
            => new ResolvedTypeDescriptor(setter.GetResultType()).TryGetArrayElementOrIEnumerableType() is not null;
    }
}
