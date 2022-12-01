using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using System.Reflection;
using System.IO;
using DotVVM.Framework.Compilation.Styles;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.Infrastructure;

public static class StyleMatchContextExtensionMethods
{
    /// <summary> Ensures that control is of type <typeparamref name="T"/>, otherwise throws an exception. </summary>
    public static IStyleMatchContext<T> Cast<T>(this IStyleMatchContext x) =>
        x is IStyleMatchContext<T> xx ? xx :
        new StyleMatchContext<T>(x.Parent, x.Control, x.Configuration);

    /// <summary> Checks if the control is of type <typeparamref name="T"/> (or of a derived type) </summary>
    public static bool IsType<T>(this IStyleMatchContext x) =>
        typeof(T).IsAssignableFrom(x.Control.Metadata.Type);
    /// <summary> Checks if the control is of type <paramref name="type"/> (or of a derived type) </summary>
    public static bool IsType(this IStyleMatchContext x, Type type) =>
        type.IsAssignableFrom(x.Control.Metadata.Type);
    /// <summary> Checks if the control is of type <typeparamref name="T"/> (or of a derived type). In case it is, the <see cref="IStyleMatchContext{T}" /> is available from the result parameter. </summary>
    public static bool IsType<T>(this IStyleMatchContext x, [NotNullWhen(true)] out IStyleMatchContext<T>? result)
    {
        if (x is IStyleMatchContext<T> xx)
        {
            result = xx;
            return true;
        }
        if (typeof(T).IsAssignableFrom(x.Control.Metadata.Type))
        {
            result = new StyleMatchContext<T>(x.Parent, x.Control, x.Configuration);
            return true;
        }
        result = null;
        return false;
    }
    /// <summary> Convert the IStyleMatchContext to control type <typeparamref name="T"/>. If it's not possible, returns null. </summary>
    public static IStyleMatchContext<T>? AsType<T>(this IStyleMatchContext x) =>
        x.IsType<T>(out var xx) ? xx : null;

    /// <summary> Returns a list of all ancestor controls - the parent, parent of the parent, ... up to the tree root. </summary>
    public static IEnumerable<IStyleMatchContext> GetAncestors(this IStyleMatchContext c)
    {
        var p = c.Parent;
        while (p != null)
        {
            yield return p;
            p = p.Parent;
        }
    }

    /// <summary> Returns a list of all ancestor controls - the parent, parent of the parent, ... up to the tree root. Filters only those of type <typeparamref name="T"/>. </summary>
    public static IEnumerable<IStyleMatchContext<T>> AncestorsOfType<T>(this IStyleMatchContext c)
    {
        var p = c.Parent;
        while (p != null)
        {
            if (p.IsType<T>(out var cc))
                yield return cc;
            p = p.Parent;
        }
    }

    /// <summary>
    /// Determines whether the control has an ancestor of the given type.
    /// </summary>
    public static bool HasAncestor<T>(this IStyleMatchContext c)
        where T : DotvvmBindableObject =>
        c.HasAncestor(typeof(T));
    
    /// <summary>
    /// Determines whether the control has an ancestor of the <paramref name="parentType"/> type.
    /// </summary>
    public static bool HasAncestor(this IStyleMatchContext c, Type parentType)
    {
        return c.GetAncestors().Any(a => a.Control.Metadata.Type == parentType);
    }

    /// <summary>
    /// Determines whether the control has an ancestor of the given type matching the given filter.
    /// </summary>
    public static bool HasAncestor<T>(this IStyleMatchContext c, Func<IStyleMatchContext<T>, bool> filter)
        where T : DotvvmBindableObject =>
        c.AncestorsOfType<T>().Any(filter);

    /// <summary>
    /// Determines whether the control has an ancestor matching the given filter.
    /// </summary>
    public static bool HasAncestor(this IStyleMatchContext c, Func<IStyleMatchContext, bool> filter) =>
        c.GetAncestors().Any(filter);

    /// <summary>
    /// Determines whether the control's ancestors types correspond to those in <paramref name="parentTypes"/>.
    /// </summary>
    public static bool HasAncestorsOrdered(this IStyleMatchContext c, IEnumerable<Type> parentTypes)
    {
        using (var enumerator = parentTypes.GetEnumerator())
        {
            if (!enumerator.MoveNext()) return true;

            foreach (var parent in c.GetAncestors())
            {
                if (parent.Control.Metadata.Type == enumerator.Current)
                {
                    if (!enumerator.MoveNext()) return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Determines whether the control's ancestors types correspond to those given.
    /// </summary>
    public static bool HasAncestorsOrdered(this IStyleMatchContext c, params Type[] parentTypes)
    {
        return c.HasAncestorsOrdered(parentTypes as IEnumerable<Type>);
    }

    /// <summary>
    /// Determines whether the control's parent's type is assignable to<typeparamref name="T"/>.
    /// </summary>
    public static bool HasParent<T>(this IStyleMatchContext c)
        where T : DotvvmControl =>
        c.HasParent<T>(out _);

    /// <summary>
    /// Determines whether the control's parent's type is assignable to <typeparamref name="T"/> and returns it.
    /// </summary>
    public static bool HasParent<T>(this IStyleMatchContext c, [NotNullWhen(true)] out IStyleMatchContext<T>? result)
        where T : DotvvmControl
    {
        result = null;
        return c.Parent?.IsType<T>(out result) == true;
    }

    /// <summary>
    /// Determines whether the control has the given <see cref="DotvvmProperty"/>.
    /// </summary>
    public static bool HasProperty(this IStyleMatchContext c, DotvvmProperty property)
    {
        return c.Control.Properties.ContainsKey(property);
    }

    /// <summary>
    /// Determines whether the control has the given <see cref="DotvvmProperty"/>.
    /// </summary>
    public static bool HasProperty<TControl>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, object?>> property)
    {
        var prop = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(property);
        return c.HasProperty(prop);
    }

    /// <summary>
    /// Determines whether the control has the given <see cref="DotvvmProperty"/>.
    /// </summary>
    public static bool HasBinding(this IStyleMatchContext c, DotvvmProperty property)
    {
        return c.Control.Properties.TryGetValue(property, out var r) && r.GetValue() is IBinding;
    }

    /// <summary>
    /// Determines whether the control has the given <see cref="DotvvmProperty"/>.
    /// </summary>
    public static bool HasBinding<TControl>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, object?>> property)
    {
        var prop = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(property);
        return c.HasBinding(prop);
    }

    /// <summary>
    /// Determines whether the control has an HTML attribute of the specified name.
    /// </summary>
    public static bool HasHtmlAttribute(this IStyleMatchContext c, string attributeName)
    {
        return c.HasPropertyGroupMember("html:", attributeName);
    }

    /// <summary>
    /// Determines whether the control has an HTML attribute of the specified name.
    /// </summary>
    public static ValueOrBinding<string>? GetHtmlAttribute(this IStyleMatchContext c, string attributeName)
    {
        return c.GetPropertyGroupMember<object>("html:", attributeName)?.UpCast<string>();
    }

    /// <summary>
    /// Determines whether the control has an HTML attribute of the specified name.
    /// </summary>
    public static string? GetHtmlAttributeValue(this IStyleMatchContext c, string attributeName)
    {
        return c.GetPropertyGroupMember<object>("html:", attributeName)?.ValueOrDefault?.ToString();
    }

    public static bool HasPropertyGroupMember(this IStyleMatchContext c, string prefix, string memberName)
    {
        var prop = c.Control.Metadata.PropertyGroups.FirstOrDefault(p => p.Prefix == prefix).PropertyGroup;
        return prop != null && c.HasProperty((DotvvmProperty)prop.GetDotvvmProperty(memberName));
    }
    
    public static ValueOrBinding<T>? GetPropertyGroupMember<T>(this IStyleMatchContext c, string prefix, string memberName)
    {
        var propGroup = c.Control.Metadata.PropertyGroups.FirstOrDefault(p => p.Prefix == prefix).PropertyGroup;
        if (propGroup is null)
            return null;
        var prop = (DotvvmProperty)propGroup.GetDotvvmProperty(memberName);
        if (!c.HasProperty(prop))
            return null;
        else return c.Property<T>(prop);
    }

    /// <summary>
    /// Determines whether the control has all the specified css classes.
    /// To be considered, classes must be explicitly specified in the DotHTML markup, classes added by controls at runtime don't count
    /// </summary>
    public static bool HasClass(this IStyleMatchContext c, params string[] classes)
    {
        var attr = c.GetHtmlAttributeValue("class")?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (attr is object)
        {
            return classes.All(c => attr.Contains(c));
        }
        return false;
    }

    /// <summary> Returns type of control this style is being applied to. </summary>
    public static Type ControlType(this IStyleMatchContext c) => c.Control.Metadata.Type;

    /// <summary> Returns tag name, if the current control is HtmlGenericControl. If the type is unknown, returns null. </summary>
    public static string? TagName(this IStyleMatchContext c) =>
        c.ControlType() == typeof(HtmlGenericControl) ? c.Control.ConstructorParameters![0] as string :
        c.IsType<ConfigurableHtmlControl>() ? c.PropertyValue<string>(ConfigurableHtmlControl.WrapperTagNameProperty) :
        c.IsType<Repeater>() ? c.PropertyValue<string>(Repeater.WrapperTagNameProperty) :
        null;

    /// <summary> Gets the property value or null if it's not defined. </summary>
    [return: MaybeNull]
    public static T PropertyValue<T>(this IStyleMatchContext c, DotvvmProperty property)
    {
        if (c.Control.GetProperty(property) is {} s)
        {
            var value = s.GetValue();
            if (value is T or null)
                return (T?)value;
        }
        return GetDefault<T>(property);
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    [return: MaybeNull]
    public static TProp PropertyValue<TControl, TProp>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, TProp>> pp)
    {
        var property = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(pp);
        return c.PropertyValue<TProp>(property);
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    [return: MaybeNull]
    public static TProp PropertyValue<TControl, TProp>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, ValueOrBinding<TProp>>> pp)
    {
        var property = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(pp);
        return c.PropertyValue<TProp>(property);
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    public static ValueOrBinding<T> Property<T>(this IStyleMatchContext c, DotvvmProperty property)
    {
        if (c.Control.GetProperty(property) is {} s)
        {
            var value = s.GetValue();
            if (value is IBinding binding)
                return new ValueOrBinding<T>(binding);
            if (value is T or null)
                return new ValueOrBinding<T>((T)value!);
        }
        return new ValueOrBinding<T>(GetDefault<T>(property));
    }

    private static T GetDefault<T>(DotvvmProperty p)
    {
        var def = p is DotvvmCapabilityProperty ? (T)Activator.CreateInstance(p.PropertyType)! : p.DefaultValue;
        if (def is T d) return d;
        if (def is null && !typeof(T).IsValueType) return default!;
        throw new Exception($"Property {p} is probably not of type {typeof(T).Name}, its default value {def ?? "null"} is not assignable to the requested type.");
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    public static ValueOrBinding<TProp> Property<TControl, TProp>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, TProp>> pp)
    {
        var property = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(pp);
        return c.Property<TProp>(property);
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    public static ValueOrBinding<TProp> Property<TControl, TProp>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, ValueOrBinding<TProp>>> pp)
    {
        var property = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(pp);
        return c.Property<TProp>(property);
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    public static IValueBinding<TProp>? Property<TControl, TProp>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, IValueBinding<TProp>?>> pp)
    {
        var property = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(pp);
        return (IValueBinding<TProp>?) c.Property<TProp>(property).BindingOrDefault;
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    public static IValueBinding? Property<TControl>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, IValueBinding?>> pp)
    {
        var property = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(pp);
        return (IValueBinding?) c.Property<object>(property).BindingOrDefault;
    }

    private static IStyleMatchContext ChildContext(this IStyleMatchContext c, ResolvedControl control) =>
        new StyleMatchContext<DotvvmBindableObject>(c, control, c.Configuration);

    public static IStyleMatchContext[] ControlProperty(this IStyleMatchContext c, DotvvmProperty property, bool includeRawLiterals = false)
    {
        if (c.Control.GetProperty(property) is not {} setter)
            return new IStyleMatchContext[0];

        var value = setter.GetValue();
        if (value is ResolvedControl control)
            return new [] { c.ChildContext(control) };
        else if (value is IEnumerable<ResolvedControl> controls)
            return controls.Select(c.ChildContext).Where(c => !c.IsRawLiteral() || includeRawLiterals).ToArray();
        else if (value is not null && value is not IBinding)
            throw new Exception($"Property {property} contains value {value} which is not a control");
        else
            return new IStyleMatchContext[0];
    }

    public static IStyleMatchContext<T>[] ControlProperty<T>(this IStyleMatchContext c, DotvvmProperty property, bool includeRawLiterals = false)
        where T: DotvvmBindableObject =>
        c.ControlProperty(property, includeRawLiterals).ControlsOfType<T>();

    /// <summary>
    /// Gets the DataContext of the control.
    /// </summary>
    public static Type DataContext(this IStyleMatchContext c)
    {
        return c.Control.DataContextTypeStack.DataContextType;
    }

    /// <summary>
    /// Determines whether the control has DataContext of the given type.
    /// </summary>
    public static bool HasDataContext<T>(this IStyleMatchContext c)
    {
        return typeof(T).IsAssignableFrom(c.DataContext());
    }

    public static Type RootDataContext(this IStyleMatchContext c)
    {
        var current = c.Control.DataContextTypeStack;
        while (current.Parent != null)
        {
            current = current.Parent;
        }
        return current.DataContextType;
    }

    /// <summary>
    /// Determines whether the control is in a page with a ViewModel of the given type.
    /// </summary>
    public static bool HasRootDataContext<T>(this IStyleMatchContext c) =>
        typeof(T).IsAssignableFrom(c.RootDataContext());

    /// <summary>
    /// Determines whether the control is in a page whose View is in the given directory.
    /// </summary>
    public static bool HasViewInDirectory(this IStyleMatchContext c, string directoryPath)
    {
        if (directoryPath.StartsWith("~/", StringComparison.Ordinal))
        {
            directoryPath = directoryPath.Substring(2);
        }

        return c.Control.TreeRoot.FileName?.StartsWith(directoryPath, StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// Determines whether the control allows to have child components
    /// </summary>
    public static bool AllowsContent(this IStyleMatchContext c) =>
        c.Control.Metadata.IsContentAllowed || c.Control.Metadata.DefaultContentProperty is object;

    /// <summary>
    /// Returns the data types that children will have
    /// </summary>
    public static DataContextStack ChildrenDataContextStack(this IStyleMatchContext c)
    {
        if (c.Control.Metadata.DefaultContentProperty is DotvvmProperty contentProperty)
            return contentProperty.GetDataContextType(c.Control);
        else if (c.Control.Metadata.IsContentAllowed)
            return c.Control.DataContextTypeStack;
        else
            throw new Exception($"Control {c.Control.Metadata.Type} does not support content.");
    }
    /// <summary>
    /// Returns the data types that children will have
    /// </summary>
    public static Type ChildrenDataContext(this IStyleMatchContext c) =>
        c.ChildrenDataContextStack().DataContextType;

    /// <summary> Returns list of controls in the Children collection or controls in the DefaultContentProperty. </summary>
    public static IStyleMatchContext[] Children(this IStyleMatchContext c, bool includeRawLiterals = false, bool allowDefaultContentProperty = true)
    {
        if (c.Control.Metadata.IsContentAllowed)
            return c.Control.Content.Select(c.ChildContext).Where(c => !c.IsRawLiteral() || includeRawLiterals).ToArray();
        else if (allowDefaultContentProperty && c.Control.Metadata.DefaultContentProperty is {} prop)
            return c.ControlProperty(prop, includeRawLiterals);
        else
            return new IStyleMatchContext[0];
    }

    public static bool IsRawLiteral(this IStyleMatchContext c) =>
        c.Control.Metadata.Type == typeof(RawLiteral);
    public static bool IsOnlyWhitespace(this IStyleMatchContext c) =>
        c.Control.IsOnlyWhitespace();
    public static bool IsRawLiteral(this IStyleMatchContext c, [NotNullWhen(true)] out string? encodedText, [NotNullWhen(true)] out string? unencodedText)
    {
        if (c.Control.Metadata.Type == typeof(RawLiteral))
        {
            encodedText = (string)c.Control.ConstructorParameters![0];
            unencodedText = (string)c.Control.ConstructorParameters[1];
            return true;
        }
        else
        {
            encodedText = unencodedText = null;
            return false;
        }
    }

    /// <summary> Returns list of controls in the Children collection or controls in the DefaultContentProperty. Filters them to find only instances of <typeparamref name="T"/> </summary>
    public static IStyleMatchContext<T>[] Children<T>(this IStyleMatchContext c, bool filterRawLiterals = true, bool allowDefaultContentProperty = true)
        where T: DotvvmBindableObject =>
        c.Children(filterRawLiterals, allowDefaultContentProperty).ControlsOfType<T>();

    public static IStyleMatchContext<T>[] ControlsOfType<T>(this IEnumerable<IStyleMatchContext> cs)
        where T: DotvvmBindableObject
    {
        var r = new List<IStyleMatchContext<T>>();
        foreach (var c in cs)
            if (c.IsType<T>(out var cT))
                r.Add(cT);
        return r.ToArray();
    }

    /// <summary> Returns a single child of type T or null. If the control contains more than one matching child, an exception is thrown. <typeparamref name="T"/> </summary>
    public static IStyleMatchContext<T>? Child<T>(this IStyleMatchContext c)
        where T: DotvvmBindableObject =>
        c.Children<T>().SingleOrDefault();

    /// <summary> Returns all child controls - those in Children and also those in any control property. </summary>
    public static IStyleMatchContext[] AllChildren(this IStyleMatchContext c, bool includeRawLiterals = false) =>
        c.Children(includeRawLiterals, allowDefaultContentProperty: false)
        .Concat(
            c.Control.Properties
            .Where(p => p.Value is ResolvedPropertyTemplate or ResolvedPropertyControl or ResolvedPropertyControlCollection)
            .SelectMany(p => c.ControlProperty(p.Key, includeRawLiterals))
        )
        .ToArray();

    /// <summary> Returns all child controls of type <typeparamref name="T"/> - those in Children and also those in any control property. </summary>
    public static IStyleMatchContext<T>[] AllChildren<T>(this IStyleMatchContext c, bool includeRawLiterals = false)
        where T: DotvvmBindableObject =>
        c.AllChildren(includeRawLiterals).ControlsOfType<T>();

    /// <summary> Returns child controls, children child controls, ... </summary>
    public static IStyleMatchContext[] Descendants(this IStyleMatchContext c, bool includeThis = false, bool includeRawLiterals = false, bool allowDefaultContentProperty = true) =>
        new [] { c }
        .SelectRecursively(c => c.Children(includeRawLiterals, allowDefaultContentProperty))
        .Skip(includeThis ? 0 : 1)
        .ToArray();

    /// <summary> Returns child controls, children child controls, ... Filter only those of type <typeparamref name="T"/> </summary>
    public static IStyleMatchContext[] Descendants<T>(this IStyleMatchContext c, bool includeThis = false, bool includeRawLiterals = false, bool allowDefaultContentProperty = true)
        where T: DotvvmBindableObject =>
        c.Descendants(includeThis, includeRawLiterals, allowDefaultContentProperty).ControlsOfType<T>();

    /// <summary> Returns child control, children child controls, ... Unlike Descendants, this method includes all children including those in control properties. </summary>
    public static IStyleMatchContext[] AllDescendants(this IStyleMatchContext c, bool includeThis = false, bool includeRawLiterals = false) =>
        new [] { c }
        .SelectRecursively(c => c.AllChildren(includeRawLiterals))
        .Skip(includeThis ? 0 : 1)
        .ToArray();

    /// <summary> Returns child controls, children child controls, ... Filter only those of type <typeparamref name="T"/>. Unlike Descendants, this method includes all children including those in control properties. </summary>
    public static IStyleMatchContext[] AllDescendants<T>(this IStyleMatchContext c, bool includeThis = false, bool includeRawLiterals = false)
        where T: DotvvmBindableObject =>
        c.AllDescendants(includeThis, includeRawLiterals).ControlsOfType<T>();

    /// <summary> Returns the contents of Styles.Tag property or an empty array if none is specified. </summary>
    public static string[] GetTags(this IStyleMatchContext context)
    {
        if (context.Control.Properties.TryGetValue(Styles.TagProperty, out var setter) &&
            setter is ResolvedPropertyValue { Value: string[] tags })
            return tags;
        else
            return new string[0];
    }
    /// <summary> Checks that Styles.Tag property is present and contains the specified tag name. </summary>
    public static bool HasTag(this IStyleMatchContext context, string tag) =>
        GetTags(context).Contains(tag, StringComparer.OrdinalIgnoreCase);
    /// <summary> Checks that Styles.Tag property is present and contains all the specified tag names. </summary>
    public static bool HasTag(this IStyleMatchContext context, params string[] tags) =>
        GetTags(context).Intersect(tags, StringComparer.OrdinalIgnoreCase).Count()
            == tags.Distinct(StringComparer.OrdinalIgnoreCase).Count();
    /// <summary> Checks that this controls has an ancestor with a specified tag. </summary>
    public static bool HasAncestorWithTag(this IStyleMatchContext context, string tag) =>
        context.HasAncestor(a => a.HasTag(tag));
    // we don't provide HasAncestorWithTag overload with multiple tags as it would be unclear whether they all have to
    // on the same ancestor or are allowed to be separate
}
