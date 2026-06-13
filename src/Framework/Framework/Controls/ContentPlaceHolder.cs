using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

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
            ResolvePendingComposition();

            base.OnInit(context);
        }

        internal void ResolvePendingComposition()
        {
            if (ID == null) return;

            // find the nearest master page 
            var masterPage = GetAllAncestors()
                .First(a => a.IsPropertySet(Internal.PendingMasterPageCompositionsProperty, inherit: false));
            
            // check there are not multiple content placeholders with the same ID in the same master page (e.g. due to being inside a template that is instantiated multiple times)
            var resolvedIds = (HashSet<string>)masterPage.GetValue(Internal.ResolvedMasterPageCompositionIdsProperty)!;
            if (resolvedIds.Contains(ID))
            {
                throw new DotvvmControlException(this,
                    $"The ContentPlaceHolder with ID '{ID}' has already been resolved. " +
                    $"ContentPlaceHolder controls used for master page composition cannot be placed inside templates that are instantiated multiple times (e.g. Repeater, foreach).");
            }

            // find the pending composition
            var pendingList = (List<PendingMasterPageComposition>)masterPage.GetValue(Internal.PendingMasterPageCompositionsProperty)!;
            var pending = pendingList.SingleOrDefault(p => p.Content.ContentPlaceHolderID == ID);
            if (pending != null)
            {
                // remove it from the master page and from the root
                pendingList.Remove(pending);
                resolvedIds.Add(ID);

                // perform the deferred composition: wrap Content in a PlaceHolder and add it as our child
                DefaultDotvvmViewBuilder.PlaceContentInContentPlaceHolder(pending.DataContextType, this, pending.Content);
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
