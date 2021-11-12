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
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;

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

            return decorator;
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render nothing

            // we must do this validation runtime, not compile time, since Decorators are often used as
            // parameters to other controls which assign the children later.
            if (Children.Count == 0)
                throw new DotvvmControlException(this, $"{GetType().Name} must have a child control");
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render nothing
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

        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            // check that decorator only has one non-whitespace child
            var children = control.Content.Where(c => !c.IsOnlyWhitespace()).ToArray();
            foreach (var child in children.Skip(1))
            {
                yield return new ControlUsageError(
                    $"{control.Metadata.Type.Name} must have only one child control.", child.DothtmlNode);
            }
        }
        
    }
}
