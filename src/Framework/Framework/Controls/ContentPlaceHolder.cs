using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

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
