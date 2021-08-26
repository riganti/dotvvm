#nullable enable
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

namespace DotVVM.Framework.Compilation.Styles
{
    public static class ResolvedControlHelper
    {
        public static ResolvedControl FromRuntimeControl(
            DotvvmBindableObject obj,
            DataContextStack dataContext)
        {
            var type = obj.GetType();

            dataContext = obj.GetDataContextType() ?? dataContext;

            // TODO markup controls, RawLiteral, HtmlGenericControl
            if (obj is DotvvmMarkupControl)
            {
                throw new NotSupportedException($"Markup controls are currently not supported.");
            }

            var content = (obj as DotvvmControl)?.Children.Select(c => FromRuntimeControl(c, dataContext)).ToList();
            var rc = new ResolvedControl(new ControlResolverMetadata(type), null, content, dataContext);

            if (obj is RawLiteral literal)
            {
                rc.ConstructorParameters = new object[] { literal.EncodedText, literal.UnencodedText, literal.IsWhitespace };
            }
            else if (type == typeof(HtmlGenericControl) && obj is HtmlGenericControl htmlControl)
            {
                rc.ConstructorParameters = new object[] { htmlControl.TagName! };
            }
            
            foreach (var p in obj.properties)
            {
                rc.SetProperty(
                    TranslateProperty(p.Key, p.Value, dataContext),
                    replace: true
                );
            }

            DotvvmProperty.CheckAllPropertiesAreRegistered(type);

            foreach (var pg in DotvvmPropertyGroup.GetPropertyGroups(type).Where(pg => pg.PropertyGroupMode == PropertyGroupMode.ValueCollection))
            {
                var dictionary = pg.PropertyInfo.GetValue(obj) as IDictionary<string, object?>;
                if (dictionary is null) continue;
                foreach (var p in dictionary)
                {
                    var property = pg.GetDotvvmProperty(p.Key);
                    rc.SetProperty(
                        TranslateProperty(property, p.Value, dataContext),
                        replace: true
                    );
                }
            }

            return rc;
        }

        public static bool IsAllowedPropertyValue([NotNullWhen(false)] object? value) =>
            value is null ||
            ReflectionUtils.IsPrimitiveType(value.GetType()) ||
            RoslynValueEmitter.IsImmutableObject(value.GetType()) ||
            value is Array && ReflectionUtils.IsPrimitiveType(value.GetType().GetElementType());

        public static ResolvedPropertySetter TranslateProperty(DotvvmProperty property, object? value, DataContextStack dataContext)
        {
            if (value is ResolvedPropertySetter resolvedSetter)
            {
                value = resolvedSetter.GetValue();
            }

            if (value is DotvvmBindableObject valueControl)
            {
                value = FromRuntimeControl(valueControl, dataContext);
            }
            else if (value is IEnumerable<DotvvmBindableObject> valueControls)
            {
                value = valueControls.Select(c => FromRuntimeControl(c, dataContext)).ToList();
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
                    throw new Exception($"Can not set a control of type {controlType} to a property {property} of type {propType}.");
            }
            else if (value is IEnumerable<ResolvedControl> cs)
            {
                if (typeof(ITemplate).IsAssignableFrom(property.PropertyType))
                    return new ResolvedPropertyTemplate(property, cs.ToList());
                else
                    return new ResolvedPropertyControlCollection(property, cs.ToList());
            }
            else if (value is IBinding)
            {
                return new ResolvedPropertyValue(property, value);
            }
            else if (IsAllowedPropertyValue(value))
            {
                var convertedValue = ReflectionUtils.ConvertValue(value, property.PropertyType);
                return new ResolvedPropertyValue(property, convertedValue);
            }
            else
            {
                throw new NotSupportedException($"Value '{value}' of type {value.GetType()} in {property} can not be compiled into a property.");
            }
        }

        public static void SetContent(ResolvedControl control, ResolvedControl[] innerControls, StyleOverrideOptions options)
        {
            if (control.Metadata.DefaultContentProperty is DotvvmProperty defaultProp)
            {
                var setter = ResolvedControlHelper.TranslateProperty(defaultProp, innerControls, control.DataContextTypeStack);

                if (!control.SetProperty(setter, options, out var err))
                    throw new DotvvmCompilationException(err);
            }
            else if (control.Metadata.IsContentAllowed)
            {
                foreach (var c in innerControls)
                    if (!typeof(DotvvmControl).IsAssignableFrom(c.Metadata.Type))
                        throw new DotvvmCompilationException($"Control {c.Metadata.Name} can not be inserted into {control.Metadata.Name} since it does not inherit from DotvvmControl.");

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
    }
}
