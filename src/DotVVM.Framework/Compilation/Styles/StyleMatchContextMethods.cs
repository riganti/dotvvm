#nullable enable
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

public static class StyleMatchContextExtensionMethods
{
    public static IStyleMatchContext<T> Cast<T>(this IStyleMatchContext x) =>
        x is IStyleMatchContext<T> xx ? xx :
        new StyleMatchContext<T>(x.Parent, x.Control, x.Configuration);

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
    public static IStyleMatchContext<T>? AsType<T>(this IStyleMatchContext x) =>
        x.IsType<T>(out var xx) ? xx : null;

    public static IEnumerable<IStyleMatchContext> GetAncestors(this IStyleMatchContext c)
    {
        var p = c.Parent;
        while (p != null)
        {
            yield return p;
            p = p.Parent;
        }
    }

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
    public static bool HasProperty<TControl>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, object>> property)
    {
        var member = ReflectionUtils.GetMemberFromExpression(property);
        return c.Control.Properties.Any(p => p.Key.PropertyInfo == member);
    }

    /// <summary>
    /// Determines whether the control has an HTML attribute of the specified name.
    /// </summary>
    public static bool HasHtmlAttribute(this IStyleMatchContext c, string attributeName)
    {
        return c.HasPropertyGroupMember("", attributeName);
    }

    /// <summary>
    /// Determines whether the control has an HTML attribute of the specified name.
    /// </summary>
    public static string? GetHtmlAttribute(this IStyleMatchContext c, string attributeName)
    {
        return c.GetPropertyGroupMember("", attributeName)?.ToString();
    }

    public static bool HasPropertyGroupMember(this IStyleMatchContext c, string prefix, string memberName)
    {
        var prop = c.Control.Metadata.PropertyGroups.FirstOrDefault(p => p.Prefix == prefix).PropertyGroup;
        return prop != null && c.HasProperty((DotvvmProperty)prop.GetDotvvmProperty(memberName));
    }
    
    public static object? GetPropertyGroupMember(this IStyleMatchContext c, string prefix, string memberName)
    {
        var prop = c.Control.Metadata.PropertyGroups.FirstOrDefault(p => p.Prefix == prefix).PropertyGroup;
        return prop is null ? null : c.Property<object>((DotvvmProperty)prop.GetDotvvmProperty(memberName));
    }

    /// <summary>
    /// Determines whether the control has all the specified css classes.
    /// To be considered, classes must be explicitly specified in the DotHTML markup, classes added by controls at runtime don't count
    /// </summary>
    public static bool HasClass(this IStyleMatchContext c, params string[] classes)
    {
        var attr = c.GetHtmlAttribute("class")?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (attr is object)
        {
            return classes.All(c => attr.Contains(c));
        }
        return false;
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    public static T? Property<T>(this IStyleMatchContext c, DotvvmProperty property)
        where T : class
    {
        ResolvedPropertySetter s;
        if (c.Control.Properties.TryGetValue(property, out s))
        {
            return (s as ResolvedPropertyValue)?.Value as T;
        }
        return null;
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    public static T? PropertyS<T>(this IStyleMatchContext c, DotvvmProperty property)
        where T : struct
    {
        ResolvedPropertySetter s;
        if (c.Control.Properties.TryGetValue(property, out s))
        {
            return (s as ResolvedPropertyValue)?.Value as T?;
        }
        return null;
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    public static TProp? PropertyS<TControl, TProp>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, TProp>> pp)
        where TProp : struct
    {
        var member = ReflectionUtils.GetMemberFromExpression(pp);
        ResolvedPropertySetter s = c.Control.Properties.FirstOrDefault(p => p.Key.PropertyInfo == member).Value;
        return (s as ResolvedPropertyValue)?.Value as TProp?;
    }

    /// <summary> Gets the property value or null if it's not defined. </summary>
    public static TProp? Property<TControl, TProp>(this IStyleMatchContext<TControl> c, Expression<Func<TControl, TProp>> pp)
        where TProp : class
    {
        var member = ReflectionUtils.GetMemberFromExpression(pp);
        ResolvedPropertySetter s = c.Control.Properties.FirstOrDefault(p => p.Key.PropertyInfo == member).Value;
        return (s as ResolvedPropertyValue)?.Value as TProp;
    }

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
        if (c.Control.Metadata.IsContentAllowed)
            return c.Control.DataContextTypeStack;
        else if (c.Control.Metadata.DefaultContentProperty is DotvvmProperty contentProperty)
        {
            return contentProperty.GetDataContextType(c.Control);
        }
        throw new Exception($"Control {c.Control.Metadata.Type} does not support content.");
    }
    /// <summary>
    /// Returns the data types that children will have
    /// </summary>
    public static Type ChildrenDataContext(this IStyleMatchContext c) =>
        c.ChildrenDataContextStack().DataContextType;

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
