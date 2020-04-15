#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    [ControlMarkupOptions(AllowContent = true)]
    public abstract class DotvvmBindableObject
    {

        private static readonly ConcurrentDictionary<Type, DotvvmProperty[]> declaredProperties = new ConcurrentDictionary<Type, DotvvmProperty[]>();


        internal DotvvmControlProperties properties;

        /// <summary>
        /// Gets the collection of control property values.
        /// </summary>
        public DotvvmPropertyDictionary Properties =>
            new DotvvmPropertyDictionary(this);


        /// <summary>
        /// Gets or sets whether this control should be rendered on the server.
        /// </summary>
        public virtual bool RenderOnServer
        {
            get { return (RenderMode)GetValue(RenderSettings.ModeProperty)! == RenderMode.Server; }
        }

        /// <summary>
        /// Gets the parent control.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public DotvvmBindableObject? Parent { get; set; }

        // WORKAROUND: Roslyn is unable to cache the delegate itself
        private static Func<Type, DotvvmProperty[]> _dotvvmProperty_ResolveProperties = DotvvmProperty.ResolveProperties;

        /// <summary>
        /// Gets all properties declared on this class or on any of its base classes.
        /// </summary>
        protected DotvvmProperty[] GetDeclaredProperties()
        {
            return declaredProperties.GetOrAdd(GetType(), _dotvvmProperty_ResolveProperties);
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
        /// The DataContext is null in client-side templates.
        /// </summary>
        [BindingCompilationRequirements(
                optional: new[] { typeof(Binding.Properties.SimplePathExpressionBindingProperty) })]
        [MarkupOptions(AllowHardCodedValue = false)]
        public object? DataContext
        {
            get { return GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }
        public static readonly DotvvmProperty DataContextProperty =
            DotvvmProperty.Register<object, DotvvmBindableObject>(c => c.DataContext, isValueInherited: true);

        [return: MaybeNull]
        public T GetValue<T>(DotvvmProperty property, bool inherit = true)
        {
            return (T)GetValue(property, inherit)!;
        }

        internal object? EvalPropertyValue(DotvvmProperty property, object? value)
        {
            if (property.IsBindingProperty) return value;
            if (value is IBinding)
            {
                DotvvmBindableObject control = this;
                // DataContext is always bound to it's parent, setting it right here is a bit faster
                if (property == DataContextProperty)
                    control = control.Parent ?? throw new DotvvmControlException(this, "Can not set DataContext binding on the root control");
                // handle binding
                if (value is IStaticValueBinding binding)
                {
                    value = binding.Evaluate(control);
                }
                else if (value is ICommandBinding command)
                {
                    value = command.GetCommandDelegate(control);
                }
                else
                {
                    throw new NotSupportedException($"Cannot evaluate binding {value} of type {value.GetType().Name}.");
                }
            }
            return value;
        }

        /// <summary>
        /// Gets the value of a specified property.
        /// </summary>
        public virtual object? GetValue(DotvvmProperty property, bool inherit = true) =>
            EvalPropertyValue(property, GetValueRaw(property, inherit));

        /// <summary>
        /// Gets the value or a binding object for a specified property.
        /// </summary>
        public virtual object? GetValueRaw(DotvvmProperty property, bool inherit = true)
        {
            return property.GetValue(this, inherit);
        }

        public void MagicSetValue(DotvvmProperty[] keys, object[] values, int hashSeed)
        {
            this.properties.AssignBulk(keys, values, hashSeed);
        }

        /// <summary>
        /// Sets the value of a specified property.
        /// </summary>
        public virtual void SetValue(DotvvmProperty property, object? value)
        {
            var originalValue = GetValueRaw(property, false);
            // TODO: really do we want to update the value binding only if it's not a binding
            if (originalValue is IUpdatableValueBinding && !(value is BindingExpression))
            {
                // if the property contains a binding and we are not passing another binding, update the value
                ((IUpdatableValueBinding)originalValue).UpdateSource(value, this);
            }
            else
            {
                SetValueRaw(property, value);
            }
        }

        /// <summary>
        /// Sets the value or a binding to the specified property.
        /// </summary>
        public void SetValueRaw(DotvvmProperty property, object? value)
        {
            property.SetValue(this, value);
        }

        /// <summary>
        /// Gets the binding set to a specified property. Returns null if the property is not set or if the value is not a binding.
        /// </summary>
        public IBinding? GetBinding(DotvvmProperty property, bool inherit = true)
            => GetValueRaw(property, inherit) as IBinding;

        /// <summary>
        /// Gets the value binding set to a specified property. Returns null if the property is not a binding.
        /// </summary>
        public IValueBinding? GetValueBinding(DotvvmProperty property, bool inherit = true)
        {
            var binding = GetBinding(property, inherit);
            if (binding != null && !(binding is IStaticValueBinding)) // throw exception on incompatible binding types
            {
                throw new DotvvmControlException(this, "ValueBindingExpression was expected!");
            }
            return binding as IValueBinding;
        }

        public ParametrizedCode GetJavascriptValue(DotvvmProperty property, bool inherit = true) =>
            GetValueBinding(property, inherit)?.KnockoutExpression ??
            new ParametrizedCode(JavascriptCompilationHelper.CompileConstant(GetValue(property)), OperatorPrecedence.Max);

        /// <summary>
        /// Gets the command binding set to a specified property. Returns null if the property is not a binding.
        /// </summary>
        public ICommandBinding? GetCommandBinding(DotvvmProperty property, bool inherit = true)
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
        public void SetBinding(DotvvmProperty property, IBinding? binding)
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
            DotvvmBindableObject? current = this;
            while (current != null)
            {
                var binding = current.GetValueBinding(DataContextProperty, false);
                if (binding != null)
                {
                    bindings.Add(binding);
                }
                current = current.Parent;
            }

            bindings.Reverse();
            return bindings;
        }

        /// <summary>
        /// Gets the closest control binding target. Returns null if the control is not found.
        /// </summary>
        public DotvvmBindableObject? GetClosestControlBindingTarget() =>
            GetClosestControlBindingTarget(out int numberOfDataContextChanges);

        /// <summary>
        /// Gets the closest control binding target and returns number of DataContext changes since the target. Returns null if the control is not found.
        /// </summary>
        public DotvvmBindableObject? GetClosestControlBindingTarget(out int numberOfDataContextChanges) =>
            (Parent ?? this).GetClosestWithPropertyValue(out numberOfDataContextChanges, (control, _) => (bool)control.GetValue(Internal.IsControlBindingTargetProperty)!);

        /// <summary>
        /// Gets the closest control binding target and returns number of DataContext changes since the target. Returns null if the control is not found.
        /// </summary>
        public DotvvmBindableObject? GetClosestControlValidationTarget(out int numberOfDataContextChanges) =>
            GetClosestWithPropertyValue(out numberOfDataContextChanges, (c, _) => c.IsPropertySet(Validation.TargetProperty, false), includeDataContextChangeOnMatchedControl: false);


        /// <summary>
        /// Gets the closest control with specified property value and returns number of DataContext changes since the target. Returns null if the control is not found.
        /// </summary>
        public DotvvmBindableObject? GetClosestWithPropertyValue(out int numberOfDataContextChanges, Func<DotvvmBindableObject, DotvvmProperty?, bool> filterFunction, bool includeDataContextChangeOnMatchedControl = true, DotvvmProperty? delegateValue = null)
        {
            DotvvmBindableObject? current = this;
            numberOfDataContextChanges = 0;
            while (current != null)
            {
                var isMatched = false;
                if (current.GetValueBinding(DataContextProperty, false) != null)
                {
                    if (current.HasBinding(DataContextProperty) || current.HasBinding(Internal.PathFragmentProperty))
                    {
                        numberOfDataContextChanges++;
                        isMatched = true;
                    }
                }
                if (filterFunction(current, delegateValue))
                {
                    if (isMatched && !includeDataContextChangeOnMatchedControl)
                    {
                        numberOfDataContextChanges--;
                    }

                    break;
                }

                current = current.Parent;
            }
            return current;
        }

        public bool HasBinding(DotvvmProperty property)
        {
            return properties.TryGet(property, out var value) && value is IBinding;
        }
        public bool HasValueBinding(DotvvmProperty property)
        {
            return properties.TryGet(property, out var value) && value is IValueBinding;
        }

        public bool HasBinding<TBinding>(DotvvmProperty property)
            where TBinding : IBinding
        {
            return properties.TryGet(property, out var value) && value is TBinding;
        }

        /// <summary>
        /// Gets all bindings set on the control (excluding BindingProperties).
        /// </summary>
        public IEnumerable<KeyValuePair<DotvvmProperty, IBinding>> GetAllBindings()
        {
            return Properties.Where(p => p.Value is IBinding)
                .Select(p => new KeyValuePair<DotvvmProperty, IBinding>(p.Key, (IBinding)p.Value!));
        }

        /// <summary>
        /// Gets all ancestors of this control starting with the parent.
        /// </summary>
        /// <param name="incudingThis">Returns also the caller control</param>
        /// <param name="onlyWhenInChildren">Only enumerate until the parent has this control in <see cref="DotvvmControl.Children" />. Note that it may have a non-trivial performance penalty</param>
        public IEnumerable<DotvvmBindableObject> GetAllAncestors(bool incudingThis = false, bool onlyWhenInChildren = false)
        {
            var ancestor = incudingThis ? this : Parent;
            while (ancestor != null)
            {
                yield return ancestor;
                if (onlyWhenInChildren)
                {
                    if (!(ancestor.Parent is DotvvmControl parentControl && parentControl.Children.Contains(ancestor)))
                        yield break;
                }
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
