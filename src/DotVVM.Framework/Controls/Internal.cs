using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
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
            DotvvmProperty.Register<string, Internal>(() => UniqueIDProperty, isValueInherited: false);

        public static readonly DotvvmProperty IsNamingContainerProperty =
            DotvvmProperty.Register<bool, Internal>(() => IsNamingContainerProperty, defaultValue: false, isValueInherited: false);

        public static readonly DotvvmProperty IsControlBindingTargetProperty =
            DotvvmProperty.Register<bool, Internal>(() => IsControlBindingTargetProperty, defaultValue: false, isValueInherited: false);

        public static readonly DotvvmProperty IsSpaPageProperty =
            DotvvmProperty.Register<bool, Internal>(() => IsSpaPageProperty, defaultValue: false, isValueInherited: true);

        public static readonly DotvvmProperty PathFragmentProperty =
            DotvvmProperty.Register<string, Internal>(() => PathFragmentProperty);

        public static readonly DotvvmProperty MarkupFileNameProperty =
            DotvvmProperty.Register<string, Internal>(() => MarkupFileNameProperty, isValueInherited: true);

        public static readonly DotvvmProperty MarkupLineNumberProperty =
            DotvvmProperty.Register<int, Internal>(() => MarkupLineNumberProperty, defaultValue: -1, isValueInherited: false);

        public static readonly DotvvmProperty ClientIDFragmentProperty =
            DotvvmProperty.Register<string, Internal>(() => ClientIDFragmentProperty, defaultValue: null, isValueInherited: false);
        
        public static DotvvmProperty IsMasterPageCompositionFinishedProperty =
            DotvvmProperty.Register<bool, Internal>(() => IsMasterPageCompositionFinishedProperty, defaultValue: false, isValueInherited: false);

        public static DotvvmProperty RequestContextProperty =
            DotvvmProperty.Register<IDotvvmRequestContext, Internal>(() => RequestContextProperty, defaultValue: null, isValueInherited: true);

    }
}
