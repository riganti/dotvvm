using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class ResourceRequiringButton : Button
    {
        public string ResourceName
        {
            get { return (string)GetValue(ResourceNameProperty); }
            set { SetValue(ResourceNameProperty, value); }
        }
        public static readonly DotvvmProperty ResourceNameProperty
            = DotvvmProperty.Register<string, ResourceRequiringButton>(c => c.ResourceName, null);


        protected override void OnPreRender(IDotvvmRequestContext context)
        {
            if (context.IsPostBack)
            {
                context.ResourceManager.AddRequiredResource(ResourceName);
            }
            base.OnPreRender(context);
        }
    }
}
