using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Base class for all controls that decorates another control (e.g. adds attributes).
    /// </summary>
    public class Decorator : HtmlGenericControl 
    {

        protected override bool RendersHtmlTag => true;

        public Decorator() : base(null)
        {
        }

        public virtual Decorator Clone()
        {
            var decorator = (Decorator)Activator.CreateInstance(GetType()).NotNull();

            foreach (var prop in properties)
            {
                decorator.properties.Set(prop.Key, prop.Value);
            }

            foreach (var attr in Attributes)
            {
                decorator.Attributes[attr.Key] = attr.Value;
            }

            return decorator;
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // do nothing
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // do nothing
        }

        public static DotvvmControl ApplyDecorators(DotvvmControl control, IEnumerable<Decorator>? decorators)
        {
            if (decorators != null)
            {
                foreach (var decorator in decorators)
                {
                    control = ApplyDecorator(control, decorator);
                }
            }

            return control;
        }

        public static DotvvmControl ApplyDecorator(DotvvmControl control, Decorator decorator)
        {
            var decoratorInstance = decorator.Clone();
            decoratorInstance.Children.Add(control);
            return decoratorInstance;
        }
    }
}
