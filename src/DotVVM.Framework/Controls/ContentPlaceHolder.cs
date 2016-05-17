using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override void EnsureNoAttributesSet()
        {
            if (!RenderWrapperTag)
                Attributes.Remove("id");
            base.EnsureNoAttributesSet();
        }

        // TODO: static checker if has a ID
    }
}