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
        private Dictionary<string, object> controlState;



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
        /// Gets the collection of properties used to persist control state for postbacks.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public Dictionary<string, object> ControlState
        {
            get
            {
                if (controlState == null)
                {
                    controlState = new Dictionary<string, object>();
                }
                return controlState;
            }
        }

        /// <summary>
        /// Gets a value indication whether the control requires the control state.
        /// </summary>
        public virtual bool RequiresControlState
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the value from control state.
        /// </summary>
        protected internal virtual T GetControlStateValue<T>(string key, T defaultValue = default(T))
        {
            if (!RequiresControlState) return defaultValue;
            object value;
            return ControlState.TryGetValue(key, out value) ? (T)value : defaultValue;
        }

        /// <summary>
        /// Sets the value for specified control state item.
        /// </summary>
        protected internal virtual void SetControlStateValue(string key, object value)
        {
            ControlState[key] = value;
        }



        /// <summary>
        /// Gets the value of a specified property.
        /// </summary>
        public override object GetValue(RedwoodProperty property, bool inherit = true)
        {
            var value = base.GetValue(property, inherit);
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
            object originalValue;
            if (Properties.TryGetValue(property, out originalValue) && originalValue is IUpdatableBindingExpression && !(value is BindingExpression))
            {
                // if the property contains a binding and we are not passing another binding, update the value
                ((IUpdatableBindingExpression)originalValue).UpdateSource(value, this, property);
            }
            else
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
        }

        /// <summary>
        /// Gets the binding set to a specified property.
        /// </summary>
        public BindingExpression GetBinding(RedwoodProperty property, bool inherit = true)
        {
            var binding = base.GetValue(property, inherit) as BindingExpression;

            // if there is a controlProperty or controlCommand binding, evaluate it
            while (binding != null && !(binding is ValueBindingExpression || binding is CommandBindingExpression))
            {
                binding = binding.Evaluate(this, property) as BindingExpression;
            }

            return binding;
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
            var dataContextBinding = GetBinding(DataContextProperty, false);
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


        /// <summary>
        /// Gets the data context hierarchy.
        /// </summary>
        internal IEnumerable<ValueBindingExpression> GetDataContextHierarchy()
        {
            var bindings = new List<ValueBindingExpression>();
            RedwoodControl current = this;
            do
            {
                if (current is RedwoodBindableControl)
                {
                    var binding = ((RedwoodBindableControl)current).GetBinding(DataContextProperty, false) as ValueBindingExpression;
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
        public RedwoodControl GetClosestControlBindingTarget()
        {
            int numberOfDataContextChanges;
            return GetClosestControlBindingTarget(out numberOfDataContextChanges);
        }

        /// <summary>
        /// Gets the closest control binding target.
        /// </summary>
        public RedwoodControl GetClosestControlBindingTarget(out int numberOfDataContextChanges)
        {
            var current = (RedwoodControl)this;
            numberOfDataContextChanges = 0;
            while (current != null)
            {
                if (current is RedwoodBindableControl && ((RedwoodBindableControl)current).GetBinding(RedwoodBindableControl.DataContextProperty, false) != null)
                {
                    numberOfDataContextChanges++;
                }
                if ((bool)current.GetValue(Internal.IsControlBindingTargetProperty))
                {
                    break;
                }

                current = current.Parent;
            }
            if (current == null)
            {
                throw new Exception("The {controlProperty: ...} binding can be only used in a markup control."); // TODO: exception handling
            }
            return current;
        }
    }
}
