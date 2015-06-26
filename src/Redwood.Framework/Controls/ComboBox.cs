using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;
using Redwood.Framework.Utils;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders a HTML drop-down list.
    /// </summary>
    public class ComboBox : SelectHtmlControlBase
    {

        [MarkupOptions(AllowBinding = false)]
        public string EmptyItemText
        {
            get { return (string) GetValue(EmptyItemTextProperty); }
            set { SetValue(EmptyItemTextProperty, value); }
        }
        public static readonly RedwoodProperty EmptyItemTextProperty 
            = RedwoodProperty.Register<string, ComboBox>(c => c.EmptyItemText, string.Empty);



        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            if (!RenderOnServer)
            {
                if (!string.IsNullOrWhiteSpace(EmptyItemText))
                {
                    writer.AddKnockoutDataBind("optionsCaption", KnockoutHelper.MakeStringLiteral(EmptyItemText));
                }
            }

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (RenderOnServer)
            {
                if (!string.IsNullOrWhiteSpace(EmptyItemText))
                {
                    writer.RenderBeginTag("option");
                    writer.WriteText(EmptyItemText);
                    writer.RenderEndTag();
                }
            }

            base.RenderContents(writer, context);
        }
    }
}
