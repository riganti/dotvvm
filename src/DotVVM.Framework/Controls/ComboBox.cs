using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
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
        /// Text displayed when no value is selected.
        /// </summary>
        public string EmptyItemText
        {
            get { return (string)GetValue(EmptyItemTextProperty); }
            set { SetValue(EmptyItemTextProperty, value); }
        }
        public static readonly DotvvmProperty EmptyItemTextProperty
            = DotvvmProperty.Register<string, ComboBox>(c => c.EmptyItemText, string.Empty);

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (IsPropertySet(EmptyItemTextProperty))
            {
                writer.AddKnockoutDataBind("optionsCaption", this.GetKnockoutBindingExpression(EmptyItemTextProperty));
            }

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderOnServer)
            {
                if (!string.IsNullOrWhiteSpace(EmptyItemText))
                {
                    writer.WriteTextOrBinding(this, EmptyItemTextProperty, wrapperTag: "option");
                }
            }

            base.RenderContents(writer, context);
        }     
    }
}
