using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
	/// <summary>
	/// Renders <c>select</c> HTML element control.
	/// </summary>
    public abstract class SelectHtmlControlBase : Selector
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
            DotvvmProperty.Register<bool, SelectHtmlControlBase>(t => t.Enabled, true);


        /// <summary>
        /// Initializes a new instance of the <see cref="SelectHtmlControlBase"/> class.
        /// </summary>
        public SelectHtmlControlBase()
			: base("select")
        {
            
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("enable", this, EnabledProperty, () =>
            {
                if (!Enabled)
                {
                    writer.AddAttribute("disabled", "disabled");
                }
            });

            writer.AddKnockoutDataBind("options", this, DataSourceProperty, renderEvenInServerRenderingMode: true);
            if (!string.IsNullOrEmpty(DisplayMember))
            {
                writer.AddKnockoutDataBind("optionsText", KnockoutHelper.MakeStringLiteral(DisplayMember));
            }
            if (!string.IsNullOrEmpty(ValueMember))
            {
                writer.AddKnockoutDataBind("optionsValue", KnockoutHelper.MakeStringLiteral(ValueMember));
            }

			// changed event
			var selectionChangedBinding = GetCommandBinding(SelectionChangedProperty);
			if (selectionChangedBinding != null)
			{
				writer.AddAttribute("onchange", KnockoutHelper.GenerateClientPostBackScript(nameof(SelectionChanged), selectionChangedBinding, context, this, isOnChange: true,useWindowSetTimeout:true));
			}

            // selected value
            writer.AddKnockoutDataBind("value", this, SelectedValueProperty, renderEvenInServerRenderingMode: true);

            base.AddAttributesToRender(writer, context);
        }
        
	}
}
