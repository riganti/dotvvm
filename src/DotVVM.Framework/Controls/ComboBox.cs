using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the HTML drop-down list.
    /// </summary>
    public class ComboBox : SelectHtmlControlBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled and can be modified.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmProperty.Register<bool, ComboBox>(t => t.Enabled, true);

        /// <summary>
        /// Gets or sets a text of an empty item. This item is auto-generated and is not part of the DataSource collection. The empty item has a value of null.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string EmptyItemText
        {
            get { return (string) GetValue(EmptyItemTextProperty); }
            set { SetValue(EmptyItemTextProperty, value); }
        }
        public static readonly DotvvmProperty EmptyItemTextProperty 
            = DotvvmProperty.Register<string, ComboBox>(c => c.EmptyItemText, string.Empty);



        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("enable", this, EnabledProperty, () =>
            {
                if (!Enabled)
                {
                    writer.AddAttribute("disabled", "disabled");
                }
            });
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
