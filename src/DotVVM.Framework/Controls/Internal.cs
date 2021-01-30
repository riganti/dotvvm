#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Contains properties that are intended for internal use.
    /// </summary>
    [ContainsDotvvmProperties]
    public class Internal
    {
        public static readonly DotvvmProperty UniqueIDProperty =
            DotvvmProperty.Register<string?, Internal>(() => UniqueIDProperty, isValueInherited: false);

        public static readonly DotvvmProperty IsNamingContainerProperty =
            DotvvmProperty.Register<bool, Internal>(() => IsNamingContainerProperty, defaultValue: false, isValueInherited: false);

        public static readonly DotvvmProperty IsControlBindingTargetProperty =
            DotvvmProperty.Register<bool, Internal>(() => IsControlBindingTargetProperty, defaultValue: false, isValueInherited: false);

        public static readonly DotvvmProperty IsSpaPageProperty =
            DotvvmProperty.Register<bool, Internal>(() => IsSpaPageProperty, defaultValue: false, isValueInherited: true);

        public static readonly DotvvmProperty UseHistoryApiSpaNavigationProperty =
            DotvvmProperty.Register<bool, Internal>(() => UseHistoryApiSpaNavigationProperty, defaultValue: true, isValueInherited: true);

        public static readonly DotvvmProperty PathFragmentProperty =
            DotvvmProperty.Register<string?, Internal>(() => PathFragmentProperty);

        /// <summary>
        /// Gets compile-time DataContextStack
        /// </summary>
        public static readonly DotvvmProperty DataContextTypeProperty =
            DotvvmProperty.Register<DataContextStack?, Internal>(() => DataContextTypeProperty, null, isValueInherited: true);

        public static readonly DotvvmProperty MarkupFileNameProperty =
            DotvvmProperty.Register<string?, Internal>(() => MarkupFileNameProperty, isValueInherited: true);

        public static readonly DotvvmProperty MarkupLineNumberProperty =
            DotvvmProperty.Register<int, Internal>(() => MarkupLineNumberProperty, defaultValue: -1, isValueInherited: false);

        public static readonly DotvvmProperty ClientIDFragmentProperty =
            DotvvmProperty.Register<string?, Internal>(() => ClientIDFragmentProperty, defaultValue: null, isValueInherited: false);

        public static DotvvmProperty IsMasterPageCompositionFinishedProperty =
            DotvvmProperty.Register<bool, Internal>(() => IsMasterPageCompositionFinishedProperty, defaultValue: false, isValueInherited: false);

        public static DotvvmProperty RequestContextProperty =
            DotvvmProperty.Register<IDotvvmRequestContext?, Internal>(() => RequestContextProperty, defaultValue: null, isValueInherited: true);

        public static DotvvmProperty CurrentIndexBindingProperty =
            DotvvmProperty.Register<IValueBinding?, Internal>(() => CurrentIndexBindingProperty);

        public static DotvvmProperty ReferencedViewModuleInfoProperty =
            DotvvmProperty.Register<ViewModuleReferenceInfo, Internal>(() => ReferencedViewModuleInfoProperty, isValueInherited: true);
    }

    public static class InternalPropertyExtensions
    {
        /// Gets an expected data context type (usually determined by the compiler)
        public static DataContextStack? GetDataContextType(this DotvvmBindableObject obj) => (DataContextStack?)obj.GetValue(Internal.DataContextTypeProperty);
        /// Sets an expected data context type
        public static void SetDataContextType(this DotvvmBindableObject obj, DataContextStack? stack) => obj.SetValue(Internal.DataContextTypeProperty, stack);
    }
}
