using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// A base class for markup controls
    /// </summary>
    public abstract class RedwoodMarkupControl : RedwoodBindableControl
    {

        /// <summary>
        /// Gets the name of the tag that wraps this markup control.
        /// </summary>
        public virtual string WrapperTagName
        {
            get { return "div"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodMarkupControl"/> class.
        /// </summary>
        public RedwoodMarkupControl()
        {
            SetValue(Internal.IsNamingContainerProperty, true);
            SetValue(Internal.IsControlBindingTargetProperty, true);
        }


        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            // handle datacontext hierarchy
            var dataContextBinding = GetBinding(DataContextProperty);
            if (dataContextBinding != null)
            {
                writer.AddKnockoutDataBind("with", dataContextBinding as ValueBindingExpression);
            }
            writer.RenderBeginTag(WrapperTagName);

            base.Render(writer, context);

            writer.RenderEndTag();
        }
    }
}
