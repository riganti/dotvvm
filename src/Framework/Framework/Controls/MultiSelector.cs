using System;
using System.Collections;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

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


        [ControlUsageValidator]
        public static new IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if (!(control.GetValue(SelectedValuesProperty) is ResolvedPropertySetter selectedValueBinding)) yield break;

            if (GetCollectionType(selectedValueBinding) is not {} selectedValueType)
            {
                yield return new(
                    $"{nameof(SelectedValues)} must be a collection.",
                    selectedValueBinding.DothtmlNode);
            }
            else if (control.GetValue(ItemValueBindingProperty) is ResolvedPropertySetter itemValueBinding)
            {
                var from = itemValueBinding.GetResultType();

                if (!IsValueAssignable(from, selectedValueType))
                {
                    yield return CreateSelectedValueTypeError(selectedValueBinding, selectedValueType, from);
                }
            }
            else if (control.GetValue(DataSourceProperty) is ResolvedPropertySetter dataSourceBinding)
            {
                if (GetCollectionType(dataSourceBinding) is not {} dataSourceType)
                {
                    yield return new ($"{nameof(DataSource)} must be a collection, but is {dataSourceBinding.GetResultType().ToCode()}.", selectedValueBinding.DothtmlNode);
                }
                else if (!IsDataSourceItemAssignable(dataSourceType, selectedValueType))
                {
                    yield return CreateSelectedValueTypeError(selectedValueBinding, selectedValueType, dataSourceType);
                }
            }
        }

        private static Type? GetCollectionType(ResolvedPropertySetter setter)
            => ResolvedTypeDescriptor.ToSystemType(new ResolvedTypeDescriptor(setter.GetResultType()).TryGetArrayElementOrIEnumerableType());
    }
}
