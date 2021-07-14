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
            else if (obj is HtmlGenericControl htmlControl)
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

            foreach (var p in DotvvmProperty.GetVirtualProperties(type))
            {
                var value = p.PropertyInfo!.GetValue(obj);
                rc.SetProperty(
                    TranslateProperty(p, value, dataContext),
                    replace: true
                );
            }

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
            value is null || ReflectionUtils.IsPrimitiveType(value.GetType()) || RoslynValueEmitter.IsImmutableObject(value.GetType());

        public static ResolvedPropertySetter TranslateProperty(DotvvmProperty property, object? value, DataContextStack dataContext)
        {
            if (value is DotvvmBindableObject valueControl)
            {
                value = FromRuntimeControl(valueControl, dataContext);
            }
            else if (value is IEnumerable<DotvvmBindableObject> valueControls)
            {
                value = valueControls.Select(c => FromRuntimeControl(c, dataContext)).ToList();
            }

            if (value is ResolvedControl c)
            {
                var propType = property.PropertyType;
                var controlType = c.Metadata.Type;
                if (typeof(ITemplate).IsAssignableFrom(propType))
                    return new ResolvedPropertyTemplate(property, new List<ResolvedControl> { c });
                else if (typeof(System.Collections.ICollection).IsAssignableFrom(propType) &&
                            ReflectionUtils.GetEnumerableType(propType)!.IsAssignableFrom(controlType))
                    return new ResolvedPropertyControlCollection(property, new List<ResolvedControl> { c });
                else if (typeof(DotvvmBindableObject).IsAssignableFrom(propType) &&
                            propType.IsAssignableFrom(controlType))
                    return new ResolvedPropertyControl(property, c);
                else
                    throw new Exception($"Can not set a control of type {controlType} to a property of type {propType}.");
            }
            else if (value is IEnumerable<ResolvedControl> cs)
            {
                if (typeof(ITemplate).IsAssignableFrom(property.PropertyType))
                    return new ResolvedPropertyTemplate(property, cs.ToList());
                else
                    return new ResolvedPropertyControlCollection(property, cs.ToList());
            }
            else if (IsAllowedPropertyValue(value))
            {
                return new ResolvedPropertyValue(property, value);
            }
            else
            {
                throw new NotSupportedException($"Value '{value}' of type {value.GetType()} in {property} can not be compiled into a property.");
            }
        }
    }
}
