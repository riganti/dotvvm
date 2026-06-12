using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a placeholder in the master page that contains the Content from the content page.
    /// </summary>
    public class ContentPlaceHolder : ConfigurableHtmlControl
    {
        public ContentPlaceHolder()
            : base(null)
        {
            SetValue(Internal.IsNamingContainerProperty, true);
        }

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            // Check if there are any pending Content controls waiting for this ContentPlaceHolder.
            // This handles the case where ContentPlaceHolder is inside a CompositeControl template
            // and is instantiated in the Load phase (after the initial master page composition).
            ResolvePendingComposition();

            base.OnInit(context);
        }

        /// <summary>
        /// Looks for a pending master page composition matching this ContentPlaceHolder's ID
        /// and performs the composition if found. Throws if the same ContentPlaceHolder ID
        /// is being resolved for a second time (e.g. ContentPlaceHolder inside a Repeater template).
        /// </summary>
        internal void ResolvePendingComposition()
        {
            if (ID == null) return;

            // Traverse ancestors to find the pending compositions list stored on the root page
            var rootPage = this.GetAllAncestors()
                .FirstOrDefault(ancestor => ancestor.GetValue(Internal.PendingMasterPageCompositionsProperty) != null);

            if (rootPage == null) return;

            var pendingList = (List<PendingMasterPageComposition>?)rootPage.GetValue(Internal.PendingMasterPageCompositionsProperty);
            if (pendingList == null) return;

            // Check for duplicate: if this ID was already resolved via deferred composition, a second
            // instantiation (e.g. ContentPlaceHolder inside a Repeater) would silently render with the
            // wrong content. Throw instead to surface the problem early.
            var resolvedIds = (HashSet<string>?)rootPage.GetValue(Internal.ResolvedMasterPageCompositionIdsProperty);
            if (resolvedIds != null && resolvedIds.Contains(ID))
            {
                throw new DotvvmControlException(this,
                    $"The ContentPlaceHolder with ID '{ID}' has already been resolved. " +
                    $"ContentPlaceHolder controls used for master page composition cannot be placed inside templates that are instantiated multiple times (e.g. Repeater, foreach).");
            }

            // When the same ID is used at multiple master page levels, the pending list contains
            // multiple entries with the same ID. Items are added from innermost to outermost (because
            // BuildView processes master pages from inner to outer). We must match the LAST entry
            // so that the outermost ContentPlaceHolder gets the outermost Content, and inner
            // ContentPlaceHolders (nested inside that content) get the inner Content entries.
            var pendingIndex = pendingList.FindLastIndex(p => p.Content.ContentPlaceHolderID == ID);
            if (pendingIndex >= 0)
            {
                var pending = pendingList[pendingIndex];
                pendingList.RemoveAt(pendingIndex);

                // Track that this ID has been resolved so a second instantiation can be detected.
                if (resolvedIds == null)
                {
                    resolvedIds = new HashSet<string>(StringComparer.Ordinal);
                    rootPage.SetValue(Internal.ResolvedMasterPageCompositionIdsProperty, resolvedIds);
                }
                resolvedIds.Add(ID);

                // Perform the deferred composition: wrap Content in a PlaceHolder and add it as our child
                var wrapper = new PlaceHolder();
                wrapper.SetDataContextType(pending.DataContextType);

                this.Children.Clear();
                this.Children.Add(wrapper);

                wrapper.Children.Add(pending.Content);
                pending.Content.SetValue(Internal.IsMasterPageCompositionFinishedProperty, true);
            }
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // The ID is used only at runtime to find the PlaceHolder-Content pair.
            // We don't want to render it
            ID = null;

            base.AddAttributesToRender(writer, context);
        }

        // TODO: static checker if has a ID
    }
}
