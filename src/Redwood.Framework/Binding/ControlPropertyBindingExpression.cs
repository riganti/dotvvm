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
            // find the parent markup control and calculate number of DataContext changes
            var current = (RedwoodControl)control;
            var level = 0;
            while (current != null)
            {
                if (current is RedwoodBindableControl && ((RedwoodBindableControl)current).GetBinding(RedwoodBindableControl.DataContextProperty, false) != null)
                {
                    level++;
                }
                if ((bool)current.GetValue(Internal.IsControlBindingTargetProperty))
                {
                    break;
                }

                current = current.Parent;
            }
            if (current == null)
            {
                throw new Exception("The {controlProperty: ...} binding can be only used in a markup control.");        // TODO: exception handling
            }

            // get the property
            var sourceProperty = RedwoodProperty.ResolveProperty(current.GetType(), Expression);
            if (sourceProperty == null)
            {
                throw new Exception(string.Format("The markup control of type '{0}' does not have a property '{1}'!", current.GetType(), Expression));        // TODO: exception handling
            }

            // check whether the property contains binding
            if (current is RedwoodBindableControl)
            {
                // there is a binding, create a new one and add _parent clauses to make it evaluate against the same DataContext
                var originalBinding = ((RedwoodBindableControl)current).GetBinding(sourceProperty);
                if (originalBinding != null)
                {
                    return new ValueBindingExpression(string.Join(".", Enumerable.Range(0, level).Select(i => "_parent").Concat(new[] { originalBinding.Expression })));
                }
            }

            // otherwise evaluate on server
            return current.GetValue(sourceProperty);
        }

        public override string TranslateToClientScript()
        {
            throw new InvalidOperationException("The {controlProperty: ...} binding cannot be translated to client script!");
        }
    }
}
