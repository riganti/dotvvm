using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// The base class for controls operating on a hierarchical collection.
    /// </summary>
    public abstract class HierarchyItemsControl : ItemsControl
    {
        protected HierarchyItemsControl()
        {
        }

        protected HierarchyItemsControl(string tagName) : base(tagName)
        {
        }

        /// <summary>
        /// Gets or sets the binding which retrieves children of a data item.
        /// </summary>
        [CollectionElementDataContextChange(1)]
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        [BindingCompilationRequirements(new[] { typeof(DataSourceAccessBinding) }, new[] { typeof(DataSourceLengthBinding) })]
        [MarkupOptions(Required = true)]
        public IValueBinding<IEnumerable<object>>? ItemChildrenBinding
        {
            get => (IValueBinding<IEnumerable<object>>?)GetValue(ItemChildrenBindingProperty);
            set => SetValue(ItemChildrenBindingProperty, value);
        }

        public static readonly DotvvmProperty ItemChildrenBindingProperty
            = DotvvmProperty.Register<IValueBinding<IEnumerable<object>>?, HierarchyItemsControl>(t => t.ItemChildrenBinding);

        /// <summary>
        /// Returns an enumeration of children of the given item.
        /// </summary>
        /// <param name="itemContainer">The item to get the children for.</param>
        protected IEnumerable<object> GetItemChildren(DotvvmControl itemContainer)
        {
            return ItemChildrenBinding?.Evaluate(itemContainer) ?? Enumerable.Empty<object>();
        }

        /// <summary>
        /// Returns whether the given item has any children.
        /// </summary>
        /// <param name="itemContainer">The item to check the children for.</param>
        protected virtual bool HasItemChildren(DotvvmControl itemContainer)
        {
            return GetItemChildren(itemContainer).Any();
        }

        /// <summary>
        /// Returns a binding used to check whether children are empty.
        /// </summary>
        protected virtual IValueBinding GetAreChildrenEmptyBinding()
        {
            return (IValueBinding)GetAreChildrenNotEmptyBinding()
                .GetProperty<NegatedBindingExpression>().Binding;
        }

        /// <summary>
        /// Returns a binding used to check whether children are not empty.
        /// </summary>
        protected virtual IValueBinding GetAreChildrenNotEmptyBinding()
        {
            return (IValueBinding)ItemChildrenBinding!
                .GetProperty<DataSourceLengthBinding>().Binding
                .GetProperty<IsMoreThanZeroBindingProperty>().Binding;
        }

        /// <summary>
        /// Returns a binding used to access children of a data item.
        /// </summary>
        protected IValueBinding GetChildAccessBinding()
        {
            return (IValueBinding)ItemChildrenBinding!
                .GetProperty<DataSourceCurrentElementBinding>().Binding;
        }
    }
}
