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
        /// and performs the composition if found.
        /// </summary>
        internal void ResolvePendingComposition()
        {
            if (ID == null) return;

            // Traverse ancestors to find the pending compositions list stored on the root page
            foreach (var ancestor in this.GetAllAncestors())
            {
                if (ancestor.GetValue(Internal.PendingMasterPageCompositionsProperty) is List<PendingMasterPageComposition> pendingList)
                {
                    var pendingIndex = pendingList.FindIndex(p => p.Content.ContentPlaceHolderID == ID);
                    if (pendingIndex >= 0)
                    {
                        var pending = pendingList[pendingIndex];
                        pendingList.RemoveAt(pendingIndex);

                        // Perform the deferred composition: wrap Content in a PlaceHolder and add it as our child
                        var wrapper = new PlaceHolder();
                        wrapper.SetDataContextType(pending.DataContextType);

                        this.Children.Clear();
                        this.Children.Add(wrapper);

                        wrapper.Children.Add(pending.Content);
                        pending.Content.SetValue(Internal.IsMasterPageCompositionFinishedProperty, true);
                    }
                    // Found the list (even if no match) - no need to search further ancestors
                    break;
                }
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
