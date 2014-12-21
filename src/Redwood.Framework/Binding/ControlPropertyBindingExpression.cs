using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Binding
{
    public class ControlPropertyBindingExpression : BindingExpression
    {

        public ControlPropertyBindingExpression()
        {
        }

        public ControlPropertyBindingExpression(string expression)
            : base(expression)
        {
        }



        public override object Evaluate(RedwoodBindableControl control, RedwoodProperty property)
        {
            var parentMarkupControl = control.GetAllAncestors().OfType<RedwoodMarkupControl>().FirstOrDefault();
            if (parentMarkupControl == null)
            {
                throw new Exception("The {controlProperty: ...} binding can be only used in a markup control.");        // TODO: exception handling
            }

            var sourceProperty = RedwoodProperty.ResolveProperty(parentMarkupControl.GetType(), Expression);     
            if (sourceProperty == null)
            {
                throw new Exception(string.Format("The markup control of type '{0}' does not have a property '{1}'!", parentMarkupControl.GetType(), Expression));        // TODO: exception handling
            }

            return parentMarkupControl.GetValue(sourceProperty);
        }

        public override string TranslateToClientScript()
        {
            throw new InvalidOperationException("The {controlProperty: ...} binding cannot be translated to client script!");
        }
    }
}
