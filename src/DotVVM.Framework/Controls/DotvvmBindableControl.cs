using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Exceptions;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base class for all control that support data-binding.
    /// </summary>
    public abstract class DotvvmBindableControl : DotvvmControl
    {
        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        public object DataContext
        {
            get { return (object)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }
        public static readonly DotvvmProperty DataContextProperty =
            DotvvmProperty.Register<object, DotvvmBindableControl>(c => c.DataContext, isValueInherited: true);



        /// <summary>
        /// Gets a collection of all data-bindings set on this control.
        /// </summary>
        protected internal IEnumerable<KeyValuePair<DotvvmProperty, IBinding>> DataBindings
            => properties.Where(kvp => kvp.Value is IBinding).Select(kvp => new KeyValuePair<DotvvmProperty, IBinding>(kvp.Key, kvp.Value as IBinding));

        /// <summary>
        /// Gets or sets whether this control should be rendered on the server.
        /// </summary>
        protected internal virtual bool RenderOnServer
        {
            get { return (RenderMode)GetValue(RenderSettings.ModeProperty) == RenderMode.Server; }
        }

        /// <summary>
        /// Gets the value of a specified property.
        /// </summary>
        public override object GetValue(DotvvmProperty property, bool inherit = true)
        {
            var value = GetValueRaw(property, inherit);
            if (property.IsBindingProperty) return value;
            while (value is IBinding)
            {
                DotvvmBindableControl control = this;
                if(inherit && !properties.ContainsKey(property))
                {
                    int n;
                    control = (DotvvmBindableControl)GetClosestWithPropertyValue(out n, d => d is DotvvmBindableControl && d.properties != null && d.properties.ContainsKey(property));
                }
                if (value is IStaticValueBinding)
                {
                    // handle binding
                    var binding = (IStaticValueBinding)value;
                    value = binding.Evaluate(control, property);
                }
                else if (value is CommandBindingExpression)
                {
                    var binding = (CommandBindingExpression)value;
                    value = binding.GetCommandDelegate(control, property);
                }
            }
            return value;
        }

        /// <summary>
        /// Gets the value or a binding object for a specified property.
        /// </summary>
        protected virtual object GetValueRaw(DotvvmProperty property, bool inherit = true)
        {
            return base.GetValue(property, inherit);
        }

        /// <summary>
        /// Sets the value of a specified property.
        /// </summary>
        public override void SetValue(DotvvmProperty property, object value)
        {
            var originalValue = GetValueRaw(property, false);
            // TODO: really do we want to update the value binding only if it's not a binding
            if (originalValue is IUpdatableValueBinding && !(value is BindingExpression))
            {
                // if the property contains a binding and we are not passing another binding, update the value
                ((IUpdatableValueBinding)originalValue).UpdateSource(value, this, property);
            }
            else
            {
                SetValueRaw(property, value);
            }
        }

        /// <summary>
        /// Sets the value or a binding to the specified property.
        /// </summary>
        protected virtual void SetValueRaw(DotvvmProperty property, object value)
        {
            base.SetValue(property, value);
        }

        /// <summary>
        /// Gets the binding set to a specified property.
        /// </summary>
        public IBinding GetBinding(DotvvmProperty property, bool inherit = true)
            => GetValueRaw(property, inherit) as IBinding;

        /// <summary>
        /// Gets the value binding set to a specified property.
        /// </summary>
        public IValueBinding GetValueBinding(DotvvmProperty property, bool inherit = true)
        {
            var binding = GetBinding(property, inherit);
            if (binding != null && !(binding is IStaticValueBinding)) // throw exception on incompatible binding types
            {
                throw new DotvvmControlException(this, "ValueBindingExpression was expected!");
            }
            return binding as IValueBinding;
        }

        /// <summary>
        /// Gets the command binding set to a specified property.
        /// </summary>
        public ICommandBinding GetCommandBinding(DotvvmProperty property, bool inherit = true)
        {
            var binding = GetBinding(property, inherit);
            if (binding != null && !(binding is ICommandBinding))
            {
                throw new DotvvmControlException(this, "CommandBindingExpression was expected!");
            }
            return binding as ICommandBinding;
        }

        /// <summary>
        /// Sets the binding to a specified property.
        /// </summary>
        public void SetBinding(DotvvmProperty property, IBinding binding)
        {
            SetValueRaw(property, binding);
        }


        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        public override sealed void Render(IHtmlWriter writer, RenderContext context)
        {
            if (Properties.ContainsKey(PostBack.UpdateProperty))
            {
                // the control might be updated on postback, add the control ID
                EnsureControlHasId();
            }


            // render the control directly to the output
            base.Render(writer, context);
        }


        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        protected override void RenderControl(IHtmlWriter writer, RenderContext context)
        {
            // if the DataContext is set, render the "with" binding
            var dataContextBinding = GetValueBinding(DataContextProperty, false);
            if (dataContextBinding != null)
            {
                writer.WriteKnockoutWithComment(dataContextBinding.GetKnockoutBindingExpression());
            }

            base.RenderControl(writer, context);

            if (dataContextBinding != null)
            {
                writer.WriteKnockoutDataBindEndComment();
            }
        }


        /// <summary>
        /// Gets the hierarchy of all DataContext bindings from the root to current control.
        /// </summary>
        [Obsolete]
        internal IEnumerable<IValueBinding> GetDataContextHierarchy()
        {
            var bindings = new List<IValueBinding>();
            DotvvmControl current = this;
            do
            {
                if (current is DotvvmBindableControl)
                {
                    var binding = ((DotvvmBindableControl)current).GetValueBinding(DataContextProperty, false);
                    if (binding != null)
                    {
                        bindings.Add(binding);
                    }
                }
                current = current.Parent;
            }
            while (current != null);

            bindings.Reverse();
            return bindings;
        }

        /// <summary>
        /// Gets the closest control binding target.
        /// </summary>
        public DotvvmControl GetClosestControlBindingTarget()
        {
            int numberOfDataContextChanges;
            return GetClosestControlBindingTarget(out numberOfDataContextChanges);
        }

        /// <summary>
        /// Gets the closest control binding target and returns number of DataContext changes since the target.
        /// </summary>
        public DotvvmControl GetClosestControlBindingTarget(out int numberOfDataContextChanges)
        {
            var result = GetClosestWithPropertyValue(out numberOfDataContextChanges, control => (bool)control.GetValue(Internal.IsControlBindingTargetProperty));
            if (result == null)
            {
                throw new DotvvmControlException(this, "The {controlProperty: ...} binding can be only used in a markup control.");
            }
            return result;
        }

        /// <summary>
        /// Gets the closest control with specified property value and returns number of DataContext changes since the target.
        /// </summary>
        public DotvvmControl GetClosestWithPropertyValue(out int numberOfDataContextChanges, Func<DotvvmControl, bool> filterFunction)
        {
            var current = (DotvvmControl)this;
            numberOfDataContextChanges = 0;
            while (current != null)
            {
                if (current is DotvvmBindableControl && ((DotvvmBindableControl)current).GetValueBinding(DataContextProperty, false) != null)
                {
                    var bindable = (DotvvmBindableControl)current;
                    if (bindable.HasBinding(DataContextProperty) || bindable.HasBinding(Internal.PathFragmentProperty))
                    {
                        numberOfDataContextChanges++;
                    }
                }
                if (filterFunction(current))
                {
                    break;
                }

                current = current.Parent;
            }
            return current;
        }

        protected internal bool HasBinding(DotvvmProperty property)
        {
            object value;
            return Properties.TryGetValue(property, out value) && value is IBinding;
        }

        protected internal bool HasBinding<TBinding>(DotvvmProperty property)
            where TBinding : IBinding
        {
            object value;
            return Properties.TryGetValue(property, out value) && value is TBinding;
        }

        /// <summary>
        /// Gets all bindings set on the control.
        /// </summary>
        internal IEnumerable<KeyValuePair<DotvvmProperty, BindingExpression>> GetAllBindings()
        {
            return Properties.Where(p => p.Value is BindingExpression)
                .Select(p => new KeyValuePair<DotvvmProperty, BindingExpression>(p.Key, (BindingExpression)p.Value));
        }
    }
}
