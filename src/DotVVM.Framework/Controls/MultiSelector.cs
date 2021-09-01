using System.Collections;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Base class for control that allows to select some of its items.
    /// </summary>
    public abstract class MultiSelector : SelectorBase
    {
        protected MultiSelector(string tagName)
            : base(tagName)
        {
        }


        /// <summary>
        /// Gets or sets the values of selected items.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public IEnumerable? SelectedValues
        {
            get { return (IEnumerable?)GetValue(SelectedValuesProperty); }
            set { SetValue(SelectedValuesProperty, value); }
        }
        public static readonly DotvvmProperty SelectedValuesProperty =
            DotvvmProperty.Register<object?, MultiSelector>(t => t.SelectedValues);

    }
}
