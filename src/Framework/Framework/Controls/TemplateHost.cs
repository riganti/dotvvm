using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a template supplied by a resource binding or from a runtime.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class TemplateHost : DotvvmControl
    {

        /// <summary>
        /// Gets or sets the template that will be rendered inside this control.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.Attribute, Required = true)]
        public ITemplate? Template
        {
            get { return (ITemplate?)GetValue(TemplateProperty); }
            set { SetValue(TemplateProperty, value); }
        }
        public static readonly DotvvmProperty TemplateProperty
            = DotvvmProperty.Register<ITemplate, TemplateHost>(c => c.Template, null);



        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            var placeHolder = new PlaceHolder();
            Template.NotNull("TemplateHost.Template is required").BuildContent(context, placeHolder);

            // validate data context of the passed template
            var myDataContext = this.GetDataContextType()!;
            if (!CheckChildrenDataContextStackEquality(myDataContext, placeHolder.Children))
            {
                throw new DotvvmControlException(this, "Passing templates into markup controls or to controls which change the binding context, is not supported!");
            }

            Children.Add(placeHolder);

            base.OnLoad(context);
        }

        private bool CheckChildrenDataContextStackEquality(DataContextStack desiredDataContext, DotvvmControlCollection children)
        {
            return children.Select(c => c.GetDataContextType())
                .Where(t => t != null)
                .All(t => Equals(t, desiredDataContext));
        }
    }
}
