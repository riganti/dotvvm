using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Utils;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using System.Net;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Compilation.Styles
{
    public static class ResolvedControlHelper
    {
        public static ResolvedControl FromRuntimeControl(
            DotvvmBindableObject obj,
            DataContextStack dataContext,
            DotvvmConfiguration? config)
        {
            var type = obj.GetType();

            dataContext = obj.GetDataContextType() ?? dataContext;

            if (obj is DotvvmMarkupControl)
            {
                throw new NotSupportedException($"Markup controls are not supported, you can use MarkupControlContainer instead.");
            }

            if (obj is MarkupControlContainer markupControl)
            {
                if (config is null)
                    throw new NotSupportedException("Can't translate MarkupControlContainer without access to DotvvmConfiguration.");
                var path = markupControl.GetMarkupPath(config);
                var controlBuilderFactory = config.ServiceProvider.GetRequiredService<IControlBuilderFactory>();
                var (descriptor, controlBuilder) = controlBuilderFactory.GetControlBuilder(path);
                var control = new ResolvedControl(new ControlResolverMetadata(new ControlType(descriptor.ControlType, path, descriptor.DataContextType)), null, new(), dataContext);
                if (markupControl.SetProperties is object)
                {
                    var templateControl = (DotvvmMarkupControl)Activator.CreateInstance(descriptor.ControlType)!;
                    markupControl.SetProperties(templateControl);
                    foreach (var p in templateControl.properties)
                    {
                        control.SetProperty(
                            TranslateProperty(p.Key, p.Value, dataContext, config),
                            replace: true
                        );
                    }
                }
                return control;
            }

            if (obj is LazyRuntimeControl wrapper)
            {
                return wrapper.ResolvedControl;
            }

            var content = (obj as DotvvmControl)?.Children.Select(c => FromRuntimeControl(c, dataContext, config)).ToList();
            var rc = new ResolvedControl(new ControlResolverMetadata(type), null, content, dataContext);

            if (obj is RawLiteral literal)
            {
                rc.ConstructorParameters = new object[] { literal.EncodedText, literal.UnencodedText, BoxingUtils.Box(literal.IsWhitespace) };
            }
            else if (type == typeof(HtmlGenericControl) && obj is HtmlGenericControl htmlControl)
            {
                rc.ConstructorParameters = new object[] { htmlControl.TagName! };
            }

            foreach (var p in obj.properties)
            {
                rc.SetProperty(
                    TranslateProperty(p.Key, p.Value, dataContext, config),
                    replace: true
                );
            }

            DotvvmProperty.CheckAllPropertiesAreRegistered(type);
            DotvvmPropertyGroup.CheckAllPropertiesAreRegistered(type);

            return rc;
        }

        private static Type[] ImmutableContainers = new[] {
            typeof(ImmutableArray<>), typeof(ImmutableList<>), typeof(ImmutableDictionary<,>), typeof(ImmutableHashSet<>), typeof(ImmutableQueue<>), typeof(ImmutableSortedDictionary<,>), typeof(ImmutableSortedSet<>), typeof(ImmutableStack<>)
        };
        internal static bool IsImmutableObject(Type type) =>
            typeof(IBinding).IsAssignableFrom(type ?? throw new ArgumentNullException(nameof(type)))
              || type.GetCustomAttribute<HandleAsImmutableObjectInDotvvmPropertyAttribute>() is object
              || type.IsGenericType && ImmutableContainers.Contains(type.GetGenericTypeDefinition()) && type.GenericTypeArguments.All(IsImmutableObject);

        public static bool IsAllowedPropertyValue([NotNullWhen(false)] object? value) =>
            value is ValueOrBinding vob && IsAllowedPropertyValue(vob.UnwrapToObject()) ||
            value is null ||
            ReflectionUtils.IsPrimitiveType(value.GetType()) ||
            IsImmutableObject(value.GetType()) ||
            value is Array && ReflectionUtils.IsPrimitiveType(value.GetType().GetElementType()!);

        public static ResolvedPropertySetter TranslateProperty(DotvvmProperty property, object? value, DataContextStack dataContext, DotvvmConfiguration? config)
        {
            if (value is ResolvedPropertySetter resolvedSetter)
            {
                value = resolvedSetter.GetValue();
            }

            value = ValueOrBindingExtensions.UnwrapToObject(value);

            if (value is DotvvmBindableObject valueControl)
            {
                value = FromRuntimeControl(valueControl, dataContext, config);
            }
            else if (value is IEnumerable<DotvvmBindableObject> valueControls)
            {
                value = valueControls.Select(c => FromRuntimeControl(c, dataContext, config)).ToList();
            }

            if (value is IEnumerable<ResolvedControl> controlCollection && controlCollection.Count() == 1)
                value = controlCollection.First();

            if (value is ResolvedControl c)
            {
                var propType = property.PropertyType;
                var controlType = c.Metadata.Type;
                if (typeof(ITemplate).IsAssignableFrom(propType))
                    return new ResolvedPropertyTemplate(property, new List<ResolvedControl> { c });
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType) &&
                            ReflectionUtils.GetEnumerableType(propType)!.IsAssignableFrom(controlType))
                    return new ResolvedPropertyControlCollection(property, new List<ResolvedControl> { c });
                else if (typeof(DotvvmBindableObject).IsAssignableFrom(propType) &&
                            propType.IsAssignableFrom(controlType))
                    return new ResolvedPropertyControl(property, c);
                else
                    throw new Exception($"Cannot set a control of type {controlType} to a property {property} of type {propType}.");
            }
            else if (value is IEnumerable<ResolvedControl> cs)
            {
                if (typeof(ITemplate).IsAssignableFrom(property.PropertyType))
                    return new ResolvedPropertyTemplate(property, cs.ToList());
                else
                    return new ResolvedPropertyControlCollection(property, cs.ToList());
            }
            else if (value is ITemplate template)
            {
                if (template is ResolvedControlTemplate resolvedTemplate)
                    return new ResolvedPropertyTemplate(property, resolvedTemplate.Controls.ToList());

                if (template is not CloneTemplate cloneTemplate)
                    throw new Exception($"Template of type {template.GetType().Name} are not supported in server side styles, use CloneTemplate instead.");
                return new ResolvedPropertyTemplate(property, cloneTemplate.Controls.Select(c => FromRuntimeControl(c, dataContext, config)).ToList());
            }
            else if (value is IBinding binding)
            {
                var resolvedBinding = binding.GetProperty<ResolvedBinding>(ErrorHandlingMode.ReturnNull);
                return new ResolvedPropertyBinding(property, resolvedBinding ?? new ResolvedBinding(binding));
            }
            else if (value is ResolvedBinding resolvedBinding)
            {
                return new ResolvedPropertyBinding(property, resolvedBinding);
            }
            else if (IsAllowedPropertyValue(value))
            {
                var convertedValue = ReflectionUtils.ConvertValue(value, property.PropertyType);
                return new ResolvedPropertyValue(property, convertedValue);
            }
            else
            {
                throw new NotSupportedException($"Value '{value}' of type {value.GetType()} in {property} cannot be compiled into a property.");
            }
        }

        public static void SetContent(ResolvedControl control, ResolvedControl[] innerControls, StyleOverrideOptions options)
        {
            if (innerControls.Length == 0)
            {
                if (options == StyleOverrideOptions.Overwrite)
                {
                    // remove the existing value
                    control.Content.Clear();
                    if (control.Metadata.DefaultContentProperty is {} dp)
                    {
                        control.RemoveProperty(dp);
                    }
                }
            }
            else if (control.Metadata.DefaultContentProperty is {} defaultProp)
            {
                var setter = ResolvedControlHelper.TranslateProperty(defaultProp, innerControls, control.DataContextTypeStack, null);

                if (!control.SetProperty(setter, options, out var err))
                    throw new DotvvmCompilationException(err);
            }
            else if (control.Metadata.IsContentAllowed)
            {
                foreach (var c in innerControls)
                    if (!typeof(DotvvmControl).IsAssignableFrom(c.Metadata.Type))
                        throw new DotvvmCompilationException($"Control {c.Metadata.Name} cannot be inserted into {control.Metadata.Name} since it does not inherit from DotvvmControl.");

                foreach (var ic in innerControls)
                    ic.Parent = control;

                switch (options)
                {
                    case StyleOverrideOptions.Append:
                        control.Content.AddRange(innerControls);
                        break;
                    case StyleOverrideOptions.Prepend:
                        control.Content.InsertRange(0, innerControls);
                        break;
                    case StyleOverrideOptions.Overwrite:
                        control.Content.Clear();
                        control.Content.AddRange(innerControls);
                        break;
                    case StyleOverrideOptions.Ignore:
                        if (control.HasOnlyWhiteSpaceContent())
                        {
                            control.Content.Clear();
                            control.Content.AddRange(innerControls);
                        }
                        break;
                }
            }
            else
            {
                throw new DotvvmCompilationException($"Could not set content on {control.Metadata.Type} as it does not allow children and does not have a DefaultContentProperty.");
            }
        }

        static DotvvmBindableObject ToLazyRuntimeControl(this ResolvedControl c, Type expectedType, IServiceProvider services)
        {
            if (expectedType == typeof(DotvvmControl))
                return new LazyRuntimeControl(c);
            else
                return ToRuntimeControl(c, services);
        }

        static object? ToRuntimeValue(this ResolvedPropertySetter setter, IServiceProvider services)
        {
            if (setter is ResolvedPropertyValue valueSetter)
                return valueSetter.Value;
            if (setter is ResolvedPropertyBinding bindingSetter)
                return bindingSetter.Binding.Binding;

                // ResolvedPropertyTemplate value => value.Content,
                // ResolvedPropertyControl value => value.Control,
                // ResolvedPropertyControlCollection value => value.Controls,
                // ResolvedPropertyCapability value => value.ToCapabilityObject(throwExceptions: false),
            var expectedType = setter.Property.PropertyType;

            if (setter is ResolvedPropertyControl controlSetter)
                return controlSetter.Control?.ToLazyRuntimeControl(expectedType, services);
            else if (setter is ResolvedPropertyControlCollection controlCollectionSetter)
            {
                var expectedControlType = ReflectionUtils.GetEnumerableType(expectedType)!;
                return controlCollectionSetter.Controls.Select(c => c.ToLazyRuntimeControl(expectedControlType, services)).ToList();
            }
            else if (setter is ResolvedPropertyTemplate templateSetter)
            {
                return new ResolvedControlTemplate(templateSetter.Content.ToArray());
            }
            else
                throw new NotSupportedException($"Property setter {setter.GetType().Name} is not supported.");
        }


        public static DotvvmBindableObject ToRuntimeControl(this ResolvedControl c, IServiceProvider? services)
        {
            _ = services ?? throw new NotImplementedException("TODO");
            var control = (DotvvmBindableObject)ActivatorUtilities.CreateInstance(services, c.Metadata.Type, c.ConstructorParameters ?? Array.Empty<object>());

            foreach (var p in c.Properties.Values)
            {
                if (p.Property is CompileTimeOnlyDotvvmProperty)
                {
                    // preserve this property too, but we have to set it directly to the dictionary. Also, don't worry about type conversions too much.
                    control.properties.Set(p.Property, p.GetValue());
                    continue;
                }

                control.SetValueRaw(p.Property, p.ToRuntimeValue(services));
            }

            foreach (var child in c.Content)
            {
                ((DotvvmControl)control).Children.Add((DotvvmControl)child.ToLazyRuntimeControl(typeof(DotvvmControl), services));
            }

            return control;
        }

        public sealed class LazyRuntimeControl: DotvvmControl
        {
            public ResolvedControl ResolvedControl { get; set; }
            private bool initialized = false;

            public LazyRuntimeControl(ResolvedControl resolvedControl)
            {
                ResolvedControl = resolvedControl;
                LifecycleRequirements = ControlLifecycleRequirements.Init;
            }

            void InitializeChildren(IDotvvmRequestContext? context)
            {
                if (initialized) return;
                // lock just to be safe. Someone could quite reasonably expect that reading control in parallel is safe
                lock (this)
                {
                    if (initialized) return;
                    Children.Add((DotvvmControl)ResolvedControl.ToRuntimeControl(context?.Services));
                    initialized = true;
                }
            }

            public override IEnumerable<DotvvmBindableObject> GetLogicalChildren()
            {
                InitializeChildren(this.GetValue(Internal.RequestContextProperty) as IDotvvmRequestContext);
                return base.GetLogicalChildren();
            }

            protected internal override void OnInit(IDotvvmRequestContext context)
            {
                InitializeChildren(context);
            }
        }

        public sealed class ResolvedControlTemplate: ITemplate
        {
            public ResolvedControl[] Controls { get; set; }

            public ResolvedControlTemplate(ResolvedControl[] controls)
            {
                Controls = controls;
            }

            public void BuildContent(IDotvvmRequestContext context, DotvvmControl container)
            {
                foreach (var c in Controls)
                {
                    container.Children.Add((DotvvmControl)c.ToRuntimeControl(context.Services));
                }
            }
        }
    }
}
