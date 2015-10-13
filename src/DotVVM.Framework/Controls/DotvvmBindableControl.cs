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
        private Dictionary<string, object> controlState;



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



        private Dictionary<DotvvmProperty, BindingExpression> dataBindings = new Dictionary<DotvvmProperty, BindingExpression>();
        /// <summary>
        /// Gets a collection of all data-bindings set on this control.
        /// </summary>
        protected internal IReadOnlyDictionary<DotvvmProperty, BindingExpression> DataBindings
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
        protected internal virtual bool RequiresControlState
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets whether this control should be rendered on the server.
        /// </summary>
        protected internal virtual bool RenderOnServer
        {
            get { return (RenderMode)GetValue(RenderSettings.ModeProperty) == RenderMode.Server; }
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
        public override object GetValue(DotvvmProperty property, bool inherit = true)
        {
            var value = GetValueRaw(property, inherit);
            while (value is IBinding)
            {
                if (value is IStaticValueBinding)
                {
                    // handle binding
                    var binding = (IStaticValueBinding)value;
                    value = binding.Evaluate(this, property);
                }
                else if (value is CommandBindingExpression)
                {
                    var binding = (CommandBindingExpression)value;
                    value = binding.GetCommandDelegate(this, property);
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
            if (originalValue is IUpdatableValueBinding && !(value is BindingExpression))
            {
                // if the property contains a binding and we are not passing another binding, update the value
                ((IUpdatableValueBinding)originalValue).UpdateSource(value, this, property);
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
                    SetValueRaw(property, value);
                }
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
        public BindingExpression GetBinding(DotvvmProperty property, bool inherit = true)
            => GetValueRaw(property, inherit) as BindingExpression;

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

            if (context.RequestContext.IsInPartialRenderingMode && (bool)GetValue(PostBack.UpdateProperty) && !(writer is MultiHtmlWriter))
            {
                // render the control and capture the HTML
                using (var htmlBuilder = new StringWriter())
                {
                    var controlWriter = new HtmlWriter(htmlBuilder, context.RequestContext);
                    var multiWriter = new MultiHtmlWriter(writer, controlWriter);
                    base.Render(multiWriter, context);
                    context.RequestContext.PostBackUpdatedControls[ID] = htmlBuilder.ToString();
                }
            }
            else
            {
                // render the control directly to the output
                base.Render(writer, context);
            }
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
            return Properties.TryGetValue(property, out value) && value is BindingExpression;
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
