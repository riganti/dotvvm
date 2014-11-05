using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// A base class for all control that support data-binding.
    /// </summary>
    public abstract class RedwoodBindableControl : RedwoodControl
    {


        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        public object DataContext
        {
            get { return (object)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }
        public static readonly RedwoodProperty DataContextProperty =
            RedwoodProperty.Register<object, RedwoodBindableControl>(c => c.DataContext, isValueInherited: true);


        
        private Dictionary<RedwoodProperty, BindingExpression> dataBindings = new Dictionary<RedwoodProperty, BindingExpression>();
        /// <summary>
        /// Gets a collection of all data-bindings set on this control.
        /// </summary>
        public IReadOnlyDictionary<RedwoodProperty, BindingExpression> DataBindings
        {
            get { return dataBindings; }
        }



        /// <summary>
        /// Gets the value of a specified property.
        /// </summary>
        public override object GetValue(RedwoodProperty property)
        {
            var value = base.GetValue(property);
            if (value is BindingExpression)
            {
                // handle binding
                var binding = (BindingExpression)value;
                return binding.Evaluate(this, property);
            }
            return value;
        }

        /// <summary>
        /// Sets the value of a specified property.
        /// </summary>
        public override void SetValue(RedwoodProperty property, object value)
        {
            // register data-bindings when they are applied to the property
            dataBindings.Remove(property);
            if (value is BindingExpression)
            {
                dataBindings[property] = (BindingExpression)value;
            }
            else
            {
                base.SetValue(property, value);
            }
        }

        /// <summary>
        /// Gets the binding set to a specified property.
        /// </summary>
        public BindingExpression GetBinding(RedwoodProperty property)
        {
            return base.GetValue(property) as BindingExpression;
        }

        /// <summary>
        /// Sets the binding to a specified property.
        /// </summary>
        public void SetBinding(RedwoodProperty property, BindingExpression binding)
        {
            base.SetValue(property, binding);
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
                context.PathFragments.Push(dataContextBinding.Expression);
            }

            base.Render(writer, context);

            if (dataContextBinding != null)
            {
                context.PathFragments.Pop();
            }
        }

    }
}
