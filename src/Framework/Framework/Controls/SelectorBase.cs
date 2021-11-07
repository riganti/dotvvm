using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Base class for control that allows to select one or more of its items.
    /// </summary>
    public abstract class SelectorBase : ItemsControl
    {
        protected SelectorBase(string tagName)
            : base(tagName)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled and can be modified.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty)!; }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmPropertyWithFallback.Register<bool, SelectorBase>(nameof(Enabled), FormControls.EnabledProperty);

        /// <summary>
        /// The expression of DataSource item that will be displayed in the control.
        /// </summary>
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        [CollectionElementDataContextChange(1)]
        [BindingCompilationRequirements(required: new[]{ typeof(SelectorItemBindingProperty) })]
        public IValueBinding? ItemTextBinding
        {
            get { return (IValueBinding?)GetValue(ItemTextBindingProperty); }
            set { SetValue(ItemTextBindingProperty, value); }
        }
        public static readonly DotvvmProperty ItemTextBindingProperty =
            DotvvmProperty.Register<IValueBinding?, SelectorBase>(nameof(ItemTextBinding));

        /// <summary>
        /// The expression of DataSource item that will be passed to the SelectedValue property when the item is selected.
        /// </summary>
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        [CollectionElementDataContextChange(1)]
        [BindingCompilationRequirements(required: new[]{ typeof(SelectorItemBindingProperty) })]
        public IValueBinding? ItemValueBinding
        {
            get { return (IValueBinding?)GetValue(ItemValueBindingProperty); }
            set { SetValue(ItemValueBindingProperty, value); }
        }
        public static readonly DotvvmProperty ItemValueBindingProperty =
            DotvvmProperty.Register<IValueBinding?, SelectorBase>(nameof(ItemValueBinding));

        /// <summary>
        /// Gets or sets the command that will be triggered when the selection is changed.
        /// </summary>
        public Command? SelectionChanged
        {
            get { return (Command?)GetValue(SelectionChangedProperty); }
            set { SetValue(SelectionChangedProperty, value); }
        }
        public static readonly DotvvmProperty SelectionChangedProperty =
            DotvvmProperty.Register<Command?, SelectorBase>(t => t.SelectionChanged, null);

        /// <summary>
        /// The expression of DataSource item that will be placed into html title attribute.
        /// </summary>
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        [CollectionElementDataContextChange(1)]
        [BindingCompilationRequirements(required: new[] { typeof(SelectorItemBindingProperty) })]
        public IValueBinding? ItemTitleBinding
        {
            get { return (IValueBinding?)GetValue(ItemTitleBindingProperty); }
            set { SetValue(ItemTitleBindingProperty, value); }
        }
        public static readonly DotvvmProperty ItemTitleBindingProperty =
            DotvvmProperty.Register<IValueBinding?, SelectorBase>(nameof(ItemTitleBinding));

        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if (control.Properties.ContainsKey(SelectorBase.ItemValueBindingProperty) &&
                control.Properties.GetValue(SelectorBase.ItemValueBindingProperty) is ResolvedPropertyBinding valueBinding)
            {
                var t = valueBinding.Binding.ResultType;
                if (!t.IsPrimitiveTypeDescriptor())
                {
                    yield return new ControlUsageError("Return type of ItemValueBinding has to be a primitive type!", valueBinding.DothtmlNode);
                }
            }
        }
    }
}
