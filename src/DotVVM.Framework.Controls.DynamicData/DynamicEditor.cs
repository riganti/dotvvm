using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls.DynamicData
{
    /// <summary>
    /// Creates the editor for the specified property using the metadata information.
    /// </summary>
    public class DynamicEditor : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets the property that should be edited.
        /// </summary>
        public IValueBinding Property
        {
            get { return (IValueBinding)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<IValueBinding, DynamicEditor>(c => c.Property, null);


        public DynamicEditor() : base("div")
        {
        }

        protected override void OnInit(IDotvvmRequestContext context)
        {
            


            base.OnInit(context);
        }
    }
}
