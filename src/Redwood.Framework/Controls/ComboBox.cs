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
    public class ComboBox : ItemsControl
    {

        /// <summary>
        /// Gets or sets the name of property in the <see cref="ItemsControl.DataSource"/> collection that will be displayed in the <see cref="ComboBox"/>.
        /// </summary>
        public string DisplayMember
        {
            get { return (string)GetValue(DisplayMemberProperty); }
            set { SetValue(DisplayMemberProperty, value); }
        }
        public static readonly RedwoodProperty DisplayMemberProperty =
            RedwoodProperty.Register<string, ComboBox>(t => t.DisplayMember, "");

        /// <summary>
        /// Gets or sets the name of property in the <see cref="ItemsControl.DataSource"/> collection that will be passed to the <see cref="SelectedValue"/> property.
        /// </summary>
        public string ValueMember
        {
            get { return (string)GetValue(ValueMemberProperty); }
            set { SetValue(ValueMemberProperty, value); }
        }
        public static readonly RedwoodProperty ValueMemberProperty =
            RedwoodProperty.Register<string, ComboBox>(t => t.ValueMember, "");


        /// <summary>
        /// Gets or sets the value selected in the <see cref="ComboBox"/>.
        /// </summary>
        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }
        public static readonly RedwoodProperty SelectedValueProperty =
            RedwoodProperty.Register<object, ComboBox>(t => t.SelectedValue);


        /// <summary>
        /// Initializes a new instance of the <see cref="ComboBox"/> class.
        /// </summary>
        public ComboBox() : base("select")
        {
            
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            if (!RenderOnServer)
            {
                writer.AddKnockoutDataBind("options", this, DataSourceProperty, () => { });

                if (!string.IsNullOrEmpty(DisplayMember))
                {
                    writer.AddKnockoutDataBind("optionsText", KnockoutHelper.MakeStringLiteral(DisplayMember));
                }
                if (!string.IsNullOrEmpty(ValueMember))
                {
                    writer.AddKnockoutDataBind("optionsValue", KnockoutHelper.MakeStringLiteral(ValueMember));
                }
            }

            writer.AddKnockoutDataBind("value", this, SelectedValueProperty, () => { });
            
            base.AddAttributesToRender(writer, context);
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (RenderOnServer)
            {
                // render items
                foreach (var item in DataSource)
                {
                    var value = string.IsNullOrEmpty(ValueMember) ? item : ReflectionUtils.GetObjectProperty(item, ValueMember);
                    var text = string.IsNullOrEmpty(DisplayMember) ? item : ReflectionUtils.GetObjectProperty(item, DisplayMember);

                    writer.AddAttribute("value", value != null ? value.ToString() : "");
                    writer.RenderSelfClosingTag("option");
                    writer.WriteText(text != null ? text.ToString() : "");
                    writer.RenderEndTag();
                }
            }
        }
    }
}
