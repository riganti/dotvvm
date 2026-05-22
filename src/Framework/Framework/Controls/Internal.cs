using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
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

        /// <summary> Assume that the DataContext is a resource binding, and level won't be present client-side </summary>
        public static readonly DotvvmProperty IsServerOnlyDataContextProperty =
            DotvvmProperty.Register<bool, Internal>(() => IsServerOnlyDataContextProperty, defaultValue: false, isValueInherited: false);

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
            DotvvmProperty.Register<ViewModuleReferenceInfo, Internal>(() => ReferencedViewModuleInfoProperty);

        public static DotvvmProperty UsedPropertiesInfoProperty =
            DotvvmProperty.Register<ControlUsedPropertiesInfo, Internal>(() => UsedPropertiesInfoProperty);

        /// <summary>
        /// Stores a list of Content controls that have not yet been matched to their corresponding ContentPlaceHolder.
        /// This is used to support ContentPlaceHolder controls inside CompositeControl templates (Load phase).
        /// </summary>
        public static readonly DotvvmProperty PendingMasterPageCompositionsProperty =
            DotvvmProperty.Register<List<PendingMasterPageComposition>?, Internal>(() => PendingMasterPageCompositionsProperty, defaultValue: null, isValueInherited: false);

        /// <summary>
        /// Tracks ContentPlaceHolder IDs that have already been resolved via deferred master page composition.
        /// Used to detect when a ContentPlaceHolder is instantiated more than once (e.g. inside a Repeater template),
        /// which is not supported and would result in only the first instance being filled with Content.
        /// </summary>
        public static readonly DotvvmProperty ResolvedMasterPageCompositionIdsProperty =
            DotvvmProperty.Register<HashSet<string>?, Internal>(() => ResolvedMasterPageCompositionIdsProperty, defaultValue: null, isValueInherited: false);

        public static bool IsViewCompilerProperty(DotvvmProperty property)
        {
            return property.DeclaringType == typeof(Internal);
        }
    }

    public static class InternalPropertyExtensions
    {
        /// Gets an expected data context type (usually determined by the compiler)
        public static DataContextStack? GetDataContextType(this DotvvmBindableObject? obj)
        {
            for (; obj != null; obj = obj.Parent)
            {
                if (obj.properties.TryGet(Internal.DataContextTypeProperty, out var v))
                    return (DataContextStack?)v;
            }
            return null;
        }
        public static DataContextStack? GetDataContextType(this DotvvmBindableObject obj, bool inherit)
        {
            if (inherit)
                return obj.GetDataContextType();
            else if (obj.properties.TryGet(Internal.DataContextTypeProperty, out var v))
                return (DataContextStack?)v;
            else
                return null;
        }

        /// Sets an expected data context type
        public static TControl SetDataContextType<TControl>(this TControl control, DataContextStack? stack)
            where TControl : DotvvmBindableObject
        {
            control.properties.Set(Internal.DataContextTypeProperty, stack);
            return control;
        }
    }

    /// <summary>
    /// Represents a Content control that has not yet been matched to a ContentPlaceHolder during master page composition.
    /// The match is deferred until the ContentPlaceHolder is added to the control tree (e.g. when a CompositeControl builds its contents).
    /// </summary>
    internal sealed class PendingMasterPageComposition
    {
        /// <summary> The Content control waiting to be placed in a ContentPlaceHolder. </summary>
        public readonly Content Content;
        /// <summary> The DataContextStack of the Content's original parent (the child page). </summary>
        public readonly DataContextStack? DataContextType;
        /// <summary> The master page file name, used for error messages. </summary>
        public readonly string? MasterPageFile;

        public PendingMasterPageComposition(Content content, DataContextStack? dataContextType, string? masterPageFile)
        {
            Content = content;
            DataContextType = dataContextType;
            MasterPageFile = masterPageFile;
        }
    }
}
