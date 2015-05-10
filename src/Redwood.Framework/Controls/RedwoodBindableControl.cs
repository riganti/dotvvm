using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

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
        protected internal IReadOnlyDictionary<RedwoodProperty, BindingExpression> DataBindings
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
        public override object GetValue(RedwoodProperty property, bool inherit = true)
        {
            var value = base.GetValue(property, inherit);
            while (value is BindingExpression)
            {
                // handle binding
                var binding = (BindingExpression)value;
                value = binding.Evaluate(this, property);
            }
            return value;
        }

        /// <summary>
        /// Sets the value of a specified property.
        /// </summary>
        public override void SetValue(RedwoodProperty property, object value)
        {
            var originalValue = base.GetValue(property, false);
            if (originalValue is IUpdatableBindingExpression && !(value is BindingExpression))
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
        /// Gets the value binding set to a specified property.
        /// </summary>
        public ValueBindingExpression GetValueBinding(RedwoodProperty property, bool inherit = true)
        {
            var binding = GetBinding(property, inherit);
            if (binding != null && !(binding is ValueBindingExpression))
            {
                throw new Exception("ValueBindingExpression was expected!");        // TODO: exception handling
            }
            return binding as ValueBindingExpression;
        }

        /// <summary>
        /// Gets the command binding set to a specified property.
        /// </summary>
        public CommandBindingExpression GetCommandBinding(RedwoodProperty property, bool inherit = true)
        {
            var binding = GetBinding(property, inherit);
            if (binding != null && !(binding is CommandBindingExpression))
            {
                throw new Exception("CommandBindingExpression was expected!");        // TODO: exception handling
            }
            return binding as CommandBindingExpression;
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
                    var controlWriter = new HtmlWriter(htmlBuilder);
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
                context.PathFragments.Push(dataContextBinding.Expression);
                writer.WriteKnockoutDataBindComment("with", dataContextBinding.TranslateToClientScript(this, DataContextProperty));
            }

            base.RenderControl(writer, context);

            if (dataContextBinding != null)
            {
                writer.WriteKnockoutDataBindEndComment();
                context.PathFragments.Pop();
            }
        }


        /// <summary>
        /// Gets the hierarchy of all DataContext bindings from the root to current control.
        /// </summary>
        internal IEnumerable<ValueBindingExpression> GetDataContextHierarchy()
        {
            var bindings = new List<ValueBindingExpression>();
            RedwoodControl current = this;
            do
            {
                if (current is RedwoodBindableControl)
                {
                    var binding = ((RedwoodBindableControl)current).GetValueBinding(DataContextProperty, false);
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
        /// Gets the closest control binding target and returns number of DataContext changes since the target.
        /// </summary>
        public RedwoodControl GetClosestControlBindingTarget(out int numberOfDataContextChanges)
        {
            var result = GetClosestWithPropertyValue(out numberOfDataContextChanges, control => (bool)control.GetValue(Internal.IsControlBindingTargetProperty));
            if (result == null)
            {
                throw new Exception("The {controlProperty: ...} binding can be only used in a markup control."); // TODO: exception handling
            }
            return result;
        }

        /// <summary>
        /// Gets the closest control with specified property value and returns number of DataContext changes since the target.
        /// </summary>
        public RedwoodControl GetClosestWithPropertyValue(out int numberOfDataContextChanges, Func<RedwoodControl, bool> filterFunction)
        {
            var current = (RedwoodControl)this;
            numberOfDataContextChanges = 0;
            while (current != null)
            {
                if (current is RedwoodBindableControl && ((RedwoodBindableControl)current).GetValueBinding(DataContextProperty, false) != null)
                {
                    numberOfDataContextChanges++;
                }
                if (filterFunction(current))
                {
                    break;
                }

                current = current.Parent;
            }
            return current;
        }

        /// <summary>
        /// Gets all bindings set on the control.
        /// </summary>
        internal IEnumerable<KeyValuePair<RedwoodProperty, BindingExpression>> GetAllBindings()
        {
            return Properties.Where(p => p.Value is BindingExpression)
                .Select(p => new KeyValuePair<RedwoodProperty, BindingExpression>(p.Key, (BindingExpression)p.Value));
        }
    }
}
