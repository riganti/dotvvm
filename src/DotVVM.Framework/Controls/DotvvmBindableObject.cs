using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    [ControlMarkupOptions(AllowContent = true)]
    public abstract class DotvvmBindableObject
    {

        private static readonly ConcurrentDictionary<Type, IReadOnlyList<DotvvmProperty>> declaredProperties = new ConcurrentDictionary<Type, IReadOnlyList<DotvvmProperty>>();


        protected internal Dictionary<DotvvmProperty, object> properties;
        
        /// <summary>
        /// Gets the collection of control property values.
        /// </summary>
        protected internal Dictionary<DotvvmProperty, object> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new Dictionary<DotvvmProperty, object>();
                }
                return properties;
            }
        }


        /// <summary>
        /// Gets or sets whether this control should be rendered on the server.
        /// </summary>
        protected internal virtual bool RenderOnServer
        {
            get { return (RenderMode)GetValue(RenderSettings.ModeProperty) == RenderMode.Server; }
        }

        /// <summary>
        /// Gets the parent control.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public DotvvmControl Parent { get; set; }

        /// <summary>
        /// Gets all properties declared on this class or on any of its base classes.
        /// </summary>
        protected IReadOnlyList<DotvvmProperty> GetDeclaredProperties()
        {
            return declaredProperties.GetOrAdd(GetType(), DotvvmProperty.ResolveProperties);
        }
        
        /// <summary>
        /// Determines whether the specified property is set.
        /// </summary>
        public bool IsPropertySet(DotvvmProperty property, bool inherit = true)
        {
            return property.IsSet(this, inherit);
        }


        /// <summary>
        /// Gets or sets a data context for the control and its children. All value and command bindings are evaluated in context of this value.
        /// </summary>
        public object DataContext
        {
            get { return (object)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }
        public static readonly DotvvmProperty DataContextProperty =
            DotvvmProperty.Register<object, DotvvmBindableObject>(c => c.DataContext, isValueInherited: true);


        /// <summary>
        /// Gets the value of a specified property.
        /// </summary>
        public virtual object GetValue(DotvvmProperty property, bool inherit = true)
        {
            var value = GetValueRaw(property, inherit);
            if (property.IsBindingProperty) return value;
            while (value is IBinding)
            {
                DotvvmBindableObject control = this;
                if (inherit && !properties.ContainsKey(property))
                {
                    int n;
                    control = GetClosestWithPropertyValue(out n, d => d.properties != null && d.properties.ContainsKey(property));
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
        protected internal virtual object GetValueRaw(DotvvmProperty property, bool inherit = true)
        {
            return property.GetValue(this, inherit);
        }

        /// <summary>
        /// Sets the value of a specified property.
        /// </summary>
        public virtual void SetValue(DotvvmProperty property, object value)
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
            property.SetValue(this, value);
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
        /// Gets the hierarchy of all DataContext bindings from the root to current control.
        /// </summary>
        [Obsolete]
        internal IEnumerable<IValueBinding> GetDataContextHierarchy()
        {
            var bindings = new List<IValueBinding>();
            DotvvmBindableObject current = this;
            do
            {
                var binding = current.GetValueBinding(DataContextProperty, false);
                if (binding != null)
                {
                    bindings.Add(binding);
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
        public DotvvmBindableObject GetClosestControlBindingTarget()
        {
            int numberOfDataContextChanges;
            return GetClosestControlBindingTarget(out numberOfDataContextChanges);
        }

        /// <summary>
        /// Gets the closest control binding target and returns number of DataContext changes since the target.
        /// </summary>
        public DotvvmBindableObject GetClosestControlBindingTarget(out int numberOfDataContextChanges) =>
            GetClosestWithPropertyValue(out numberOfDataContextChanges, control => (bool)control.GetValue(Internal.IsControlBindingTargetProperty));

        /// <summary>
        /// Gets the closest control with specified property value and returns number of DataContext changes since the target.
        /// </summary>
        public DotvvmBindableObject GetClosestWithPropertyValue(out int numberOfDataContextChanges, Func<DotvvmBindableObject, bool> filterFunction)
        {
            var current = this;
            numberOfDataContextChanges = 0;
            while (current != null)
            {
                if (current.GetValueBinding(DataContextProperty, false) != null)
                {
                    if (current.HasBinding(DataContextProperty) || current.HasBinding(Internal.PathFragmentProperty))
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
        protected internal bool HasValueBinding(DotvvmProperty property)
        {
            object value;
            return Properties.TryGetValue(property, out value) && value is IValueBinding;
        }

        protected internal bool HasBinding<TBinding>(DotvvmProperty property)
            where TBinding : IBinding
        {
            object value;
            return Properties.TryGetValue(property, out value) && value is TBinding;
        }

        /// <summary>
        /// Gets all bindings set on the control (excluding BindingProperties).
        /// </summary>
        public IEnumerable<KeyValuePair<DotvvmProperty, BindingExpression>> GetAllBindings()
        {
            return Properties.Where(p => p.Value is BindingExpression) // && !p.Key.IsBindingProperty)
                .Select(p => new KeyValuePair<DotvvmProperty, BindingExpression>(p.Key, (BindingExpression)p.Value));
        }

        /// <summary>
        /// Gets all ancestors of this control starting with the parent.
        /// </summary>
        public IEnumerable<DotvvmControl> GetAllAncestors()
        {
            var ancestor = Parent;
            while (ancestor != null)
            {
                yield return ancestor;
                ancestor = ancestor.Parent;
            }
        }

        /// <summary>
        /// Gets the root of the control tree.
        /// </summary>
        public DotvvmBindableObject GetRoot()
        {
            if (Parent == null) return this;
            return GetAllAncestors().Last();
        }

        /// <summary>
        /// Gets the logical children of this control (including controls that are not in the visual tree but which can contain command bindings).
        /// </summary>
        public virtual IEnumerable<DotvvmBindableObject> GetLogicalChildren()
        {
            return Enumerable.Empty<DotvvmBindableObject>();
        }

        /// <summary>
        /// Copies the value of a property from this <see cref="DotvvmBindableObject"/> (source) to a property of another <see cref="DotvvmBindableObject"/> (target).
        /// </summary>
        /// <exception cref="DotvvmControlException">Gets thrown if copying fails and <paramref name="throwOnFailure"/> is set to true</exception>
        /// <param name="sourceProperty">The <see cref="DotvvmProperty"/> whose value will be copied</param>
        /// <param name="target">The <see cref="DotvvmBindableObject"/> that holds the value of the <paramref name="targetProperty"/></param>
        /// <param name="targetProperty">The <see cref="DotvvmProperty"/> to which <paramref name="sourceProperty"/> will be copied</param>
        /// <param name="throwOnFailure">Determines whether to throw an exception if copying fails</param>
        protected void CopyProperty(DotvvmProperty sourceProperty, DotvvmBindableObject target, DotvvmProperty targetProperty, bool throwOnFailure = false)
        {
            if (throwOnFailure && !targetProperty.MarkupOptions.AllowBinding && !targetProperty.MarkupOptions.AllowHardCodedValue)
            {
                throw new DotvvmControlException(this, $"TargetProperty: {targetProperty.FullName} doesn't allow bindings nor hard coded values");
            }

            if (targetProperty.MarkupOptions.AllowBinding && HasBinding(sourceProperty))
            {
                target.SetBinding(targetProperty, GetBinding(sourceProperty));
            }
            else if (targetProperty.MarkupOptions.AllowHardCodedValue && IsPropertySet(sourceProperty))
            {
                target.SetValue(targetProperty, GetValue(sourceProperty));
            }
            else if (throwOnFailure)
            {
                throw new DotvvmControlException(this, $"Value of {sourceProperty.FullName} couldn't be copied to targetProperty: {targetProperty.FullName}, because {targetProperty.FullName} is not set.");
            }
        }
    }
}