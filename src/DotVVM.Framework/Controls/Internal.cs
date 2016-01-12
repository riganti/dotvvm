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
            DotvvmProperty.Register<string, Internal>("UniqueID", isValueInherited: false);

        public static readonly DotvvmProperty IsNamingContainerProperty =
            DotvvmProperty.Register<bool, Internal>("IsNamingContainer", defaultValue: false, isValueInherited: false);

        public static readonly DotvvmProperty IsControlBindingTargetProperty =
            DotvvmProperty.Register<bool, Internal>("IsControlBindingTarget", defaultValue: false, isValueInherited: false);

        public static readonly DotvvmProperty IsSpaPageProperty =
            DotvvmProperty.Register<bool, Internal>("IsSpaPage", defaultValue: false, isValueInherited: true);

        public static readonly DotvvmProperty PathFragmentProperty =
            DotvvmProperty.Register<string, Internal>("PathFragment");

        public static readonly DotvvmProperty MarkupFileNameProperty =
            DotvvmProperty.Register<string, Internal>("MarkupFileName", isValueInherited: true);

        public static readonly DotvvmProperty MarkupLineNumberProperty =
            DotvvmProperty.Register<int, Internal>("MarkupLineNumber", defaultValue: -1, isValueInherited: false);

        public static readonly DotvvmProperty ClientIDFragmentProperty =
            DotvvmProperty.Register<string, Internal>("ClientIDFragment", defaultValue: null, isValueInherited: false);
        
        public static DotvvmProperty IsMasterPageCompositionFinishedProperty =
            DotvvmProperty.Register<bool, Internal>("IsMasterPageCompositionFinished", defaultValue: false, isValueInherited: false);

        public static DotvvmProperty RequestContextProperty =
            DotvvmProperty.Register<IDotvvmRequestContext, Internal>("RequestContext", defaultValue: null, isValueInherited: true);

    }
}
