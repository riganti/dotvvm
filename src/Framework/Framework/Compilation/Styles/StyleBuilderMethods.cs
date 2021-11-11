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
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;

public static partial class StyleBuilderExtensionMethods
{
    /// <summary> Sets a specified property on the matching controls. The referenced property must be a wrapper around a DotvvmProperty. </summary>
    public static IStyleBuilder<TControl> SetProperty<TControl, TProperty>(
        this IStyleBuilder<TControl> sb,
        Expression<Func<TControl, TProperty>> property,
        ValueOrBinding<TProperty> value,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
    {
        var dotprop = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(property);
        return sb.SetDotvvmProperty(dotprop, value, options);
    }
    /// <summary> Sets a specified property on the matching controls. The referenced property must be a wrapper around a DotvvmProperty. </summary>
    public static IStyleBuilder<TControl> SetProperty<TControl, TProperty>(
        this IStyleBuilder<TControl> sb,
        Expression<Func<TControl, ValueOrBinding<TProperty>>> property,
        ValueOrBinding<TProperty> value,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
    {
        var dotprop = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(property);
        return sb.SetDotvvmProperty(dotprop, value, options);
    }

    /// <summary> Sets a control to the specified property on the matching controls. </summary>
    /// <param name="styleBuilder">This style builder can be used to set properties on the added component.</param>
    public static T SetControlProperty<T, TControl>(
        this T sb,
        DotvvmProperty property,
        TControl prototypeControl,
        Action<StyleBuilder<TControl>>? styleBuilder = null,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        where T: IStyleBuilder
        where TControl: DotvvmBindableObject
    {
        var innerControlStyleBuilder = new StyleBuilder<TControl>(null, false);
        styleBuilder?.Invoke(innerControlStyleBuilder);

        return sb.AddApplicator(
            new PropertyControlCollectionStyleApplicator(property, options, prototypeControl, innerControlStyleBuilder.GetStyle())
        );
    }

    /// <summary> Sets a control to the specified property on the matching controls. </summary>
    /// <param name="styleBuilder">This style builder can be used to set properties on the added component.</param>
    public static IStyleBuilder<TControl> SetControlProperty<TControl, TInnerControl>(
        this IStyleBuilder<TControl> sb,
        Expression<Func<TControl, object>> property,
        TInnerControl prototypeControl,
        Action<StyleBuilder<TInnerControl>>? styleBuilder = null,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        where TInnerControl: DotvvmBindableObject
    {
        var dotprop = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(property);
        return sb.SetControlProperty<IStyleBuilder<TControl>, TInnerControl>(
            dotprop, prototypeControl, styleBuilder, options);
    }

    /// <summary> Adds a control to the specified property on the matching controls. </summary>
    /// <param name="styleBuilder">This style builder can be used to set properties on the added component.</param>
    public static IStyleBuilder<TControl> AppendControlProperty<TControl, TInnerControl>(
        this IStyleBuilder<TControl> sb,
        Expression<Func<TControl, object>> property,
        TInnerControl prototypeControl,
        Action<StyleBuilder<TInnerControl>>? styleBuilder = null)
        where TInnerControl: DotvvmBindableObject =>
        sb.SetControlProperty(property, prototypeControl, styleBuilder, StyleOverrideOptions.Append);

    /// <summary> Adds a control to the specified property on the matching controls. </summary>
    /// <param name="styleBuilder">This style builder can be used to set properties on the added component.</param>
    public static T AppendControlProperty<T, TControl>(
        this T sb,
        DotvvmProperty property,
        TControl controlPrototype,
        Action<StyleBuilder<TControl>>? styleBuilder = null)
        where T: IStyleBuilder
        where TControl: DotvvmBindableObject =>
        sb.SetControlProperty(property, controlPrototype, styleBuilder, StyleOverrideOptions.Append);

    /// <summary> Inserts an HTML control into the specified control property. </summary>
    public static T SetHtmlControlProperty<T>(
        this T sb,
        DotvvmProperty property,
        string tag,
        Action<StyleBuilder<HtmlGenericControl>>? styleBuilder = null,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        where T: IStyleBuilder
    {
        if (tag == null)
            throw new ArgumentNullException(nameof(tag));
        return sb.SetControlProperty(property, new HtmlGenericControl(tag), styleBuilder, options);
    }

    /// <summary> Inserts a literal into the specified control property. </summary>
    public static T SetLiteralControlProperty<T>(
        this T sb,
        DotvvmProperty property,
        ValueOrBinding<string> text,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        where T: IStyleBuilder =>

        sb.SetControlProperty(property, text.Process<DotvvmControl>(RawLiteral.Create, _ => new Literal(text)), options: options);

    /// <summary> Sets a specified property on the matching controls </summary>
    public static T SetDotvvmProperty<T>(
        this T sb,
        DotvvmProperty property,
        object? value,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        where T: IStyleBuilder
    {
        value = ValueOrBindingExtensions.UnwrapToObject(value);
        if (value is DotvvmBindableObject || value is IEnumerable<DotvvmBindableObject>)
            throw new ArgumentException($"For setting controls into properties please use the SetControlProperty or AppendControlProperty functions.", nameof(value));
        if (!ResolvedControlHelper.IsAllowedPropertyValue(value))
            throw new ArgumentException($"Setting value '{value}' of type {value.GetType()} is not allowed compile time. The type must be primitive.", nameof(value));
        if (value is IBinding binding)
        {
            var resultType = binding.GetProperty<ResultTypeBindingProperty>(ErrorHandlingMode.ReturnNull)?.Type;
            if (resultType != null && resultType != typeof(object) && property.PropertyType.IsAssignableFrom(resultType))
                throw new ArgumentException($"Binding {value} of type {resultType} is not assignable to property {property} of type {property.PropertyType}", nameof(value));
        }
        else if (value is object && !property.PropertyType.IsInstanceOfType(value))
        {
            throw new ArgumentException($"Value '{value}' of type {value.GetType()} is not assignable to property {property} of type {property.PropertyType}", nameof(value));
        }

        return sb.AddApplicator(new PropertyStyleApplicator(property, value, options));
    }

    /// <summary> Sets a specified property on the matching controls. The value can be computed from any property on the control using the IStyleMatchContext. </summary>
    public static IStyleBuilder<T> SetDotvvmProperty<T>(
        this IStyleBuilder<T> sb,
        DotvvmProperty property,
        Func<IStyleMatchContext<T>, object?> value,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite) =>
        
        sb.AddApplicator(new GenericPropertyStyleApplicator<T>(property, value, options));

    /// <summary> Sets a specified property on the matching controls. The value can be computed from any property on the control using the IStyleMatchContext. </summary>
    public static IStyleBuilder<TControl> SetProperty<TControl, TProperty>(
        this IStyleBuilder<TControl> sb,
        Expression<Func<TControl, TProperty>> property,
        Func<IStyleMatchContext<TControl>, TProperty> value,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
    {
        var dotprop = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(property);
        return sb.SetDotvvmProperty(dotprop, c => (object?)value(c), options);
    }

    /// <summary> Sets the specified property to a binding.
    /// Value binding is used by default, alternative binding types can be set using the bindingOptions parameter.
    /// Note that the binding is parsed according to the data context of the control, you can check that using c.HasDataContext. </summary>
    public static T SetPropertyBinding<T>(
        this T sb,
        Expression<Func<T, object>> property,
        string binding,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite,
        BindingParserOptions? bindingOptions = null)
        where T: IStyleBuilder
    {
        var dotprop = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(property);
        return sb.AddApplicator(new PropertyStyleBindingApplicator(
            dotprop,
            binding,
            options,
            bindingOptions ?? GetDefaultBindingType(dotprop),
            allowChangingBindingType: bindingOptions == null
        ));
    }

    /// <summary> Sets the specified property to a binding.
    /// Value binding is used by default, alternative binding types can be set using the bindingOptions parameter.
    /// Note that the binding is parsed according to the data context of the control, you can check that using c.HasDataContext. </summary>
    public static T SetDotvvmPropertyBinding<T>(
        this T sb,
        DotvvmProperty property,
        string binding,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite,
        BindingParserOptions? bindingOptions = null)
        where T: IStyleBuilder =>
        
        sb.AddApplicator(new PropertyStyleBindingApplicator(
            property,
            binding,
            options,
            bindingOptions ?? GetDefaultBindingType(property),
            allowChangingBindingType: bindingOptions == null
        ));

    private static BindingParserOptions GetDefaultBindingType(DotvvmProperty property)
    {
        if (!property.MarkupOptions.AllowBinding)
            return BindingParserOptions.Resource;
        else if (property.PropertyType.IsDelegate() ||
                 typeof(ICommandBinding).IsAssignableFrom(property.PropertyType))
            // If ever for commands, styles most likely are going to be used for tiny utilities like "assign one property"
            // where staticCommand makes a better default than command.
            return BindingParserOptions.StaticCommand;
        else
            return BindingParserOptions.Value;
    }

    /// <summary> Sets the specified property to a binding.
    /// Value binding is used by default, alternative binding types can be set using the bindingOptions parameter.
    /// Note that the binding is parsed according to the data context of the control, you can check that using c.HasDataContext. </summary>
    public static IStyleBuilder<TControl> SetPropertyBinding<TControl>(
        this IStyleBuilder<TControl> sb,
        Expression<Func<TControl, object>> property,
        string binding,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite,
        BindingParserOptions? bindingOptions = null)
    {
        var dotprop = DotvvmPropertyUtils.GetDotvvmPropertyFromExpression(property);
        return sb.SetDotvvmPropertyBinding(dotprop, binding, options, bindingOptions);
    }

    /// <summary> Adds a Class-className binding to the control </summary>
    public static T AddClassBinding<T>(this T sb, string className, string binding, StyleOverrideOptions options = StyleOverrideOptions.Overwrite, BindingParserOptions? bindingOptions = null)
        where T: IStyleBuilder =>

        sb.SetPropertyGroupMemberBinding("Class-", className, binding, options, bindingOptions);


    /// <summary> Appends HTML attribute of the control.
    /// Value binding is used by default, alternative binding types can be set using the bindingOptions parameter.
    /// Note that the binding is parsed according to the data context of the control, you can check that using c.HasDataContext. </summary>
    public static T AppendAttributeBinding<T>(
        this T sb,
        string attribute,
        string binding,
        BindingParserOptions? bindingOptions = null)
        where T: IStyleBuilder =>

        sb.SetPropertyGroupMemberBinding("html:", attribute, binding, StyleOverrideOptions.Append, bindingOptions);

    /// <summary> Sets HTML attribute of the control.
    /// Value binding is used by default, alternative binding types can be set using the bindingOptions parameter.
    /// Note that the binding is parsed according to the data context of the control, you can check that using c.HasDataContext. </summary>
    public static T SetAttributeBinding<T>(
        this T sb,
        string attribute,
        string binding,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite,
        BindingParserOptions? bindingOptions = null)
        where T: IStyleBuilder =>

        sb.SetPropertyGroupMemberBinding("html:", attribute, binding, options, bindingOptions);

    /// <summary> Sets property group member of the control. For example SetPropertyGroupMember("Param-", "Abcd", "_root.DefaultAbcd") would set the Abcd parameter on RouteLink.
    /// Value binding is used by default, alternative binding types can be set using the bindingOptions parameter.
    /// Note that the binding is parsed according to the data context of the control, you can check that using c.HasDataContext. </summary>
    public static T SetPropertyGroupMemberBinding<T>(
        this T sb,
        string prefix,
        string memberName,
        string binding,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite,
        BindingParserOptions? bindingOptions = null)
        where T: IStyleBuilder =>
        sb.SetDotvvmPropertyBinding(sb.GetPropertyGroup(prefix, memberName), binding, options, bindingOptions);

    /// <summary> Appends a css class (or multiple of them) to the control. </summary>
    public static T AddClass<T>(this T sb, string className)
        where T: IStyleBuilder =>

        sb.AppendAttribute("class", className);

    /// <summary> Appends a css class (or multiple of them) to the control. </summary>
    public static IStyleBuilder<T> AddClass<T>(this IStyleBuilder<T> sb, Func<IStyleMatchContext<T>, ValueOrBinding<string>> className) =>

        sb.AppendAttribute("class", className);

    /// <summary> Appends a css class (or multiple of them) with the condition. </summary>
    public static IStyleBuilder<T> AddClass<T>(this IStyleBuilder<T> sb, string className, Func<IStyleMatchContext<T>, ValueOrBinding<bool>> condition) =>

        sb.SetPropertyGroupMember("Class-", className, c => condition(c), StyleOverrideOptions.Append);

    /// <summary> Appends a css class (or multiple of them) with the condition. </summary>
    public static IStyleBuilder<T> AddClassBinding<T>(this IStyleBuilder<T> sb, string className, string conditionBinding, BindingParserOptions? bindingOptions = null) =>

        sb.SetPropertyGroupMemberBinding("Class-", className, conditionBinding, StyleOverrideOptions.Append);

    /// <summary> Appends value to the specified attribute. </summary>
    public static T AppendAttribute<T>(this T sb, string attribute, ValueOrBinding<string> value)
        where T: IStyleBuilder =>

        sb.SetPropertyGroupMember("html:", attribute, value, StyleOverrideOptions.Append);

    /// <summary> Appends value to the specified attribute. </summary>
    public static IStyleBuilder<T> AppendAttribute<T>(this IStyleBuilder<T> sb, string attribute, Func<IStyleMatchContext<T>, ValueOrBinding<string>> value) =>

        sb.SetPropertyGroupMember("html:", attribute, c => value(c), StyleOverrideOptions.Append);

    /// <summary> Sets HTML attribute of the control. </summary>
    public static T SetAttribute<T>(this T sb, string attribute, object value, StyleOverrideOptions options = StyleOverrideOptions.Ignore)
        where T: IStyleBuilder =>

        sb.SetPropertyGroupMember("html:", attribute, value, options);

    /// <summary> Sets HTML attribute of the control. </summary>
    public static IStyleBuilder<T> SetAttribute<T>(this IStyleBuilder<T> sb, string attribute, Func<IStyleMatchContext<T>, object> value, StyleOverrideOptions options = StyleOverrideOptions.Ignore) =>

        sb.SetPropertyGroupMember("html:", attribute, value, options);

    public static DotvvmProperty GetPropertyGroup(this IStyleBuilder sb, string prefix, string member)
    {
        var group = DotvvmPropertyGroup.GetPropertyGroups(sb.ControlType).Where(p => p.Prefixes.Contains(prefix)).ToArray();
        if (group.Length == 0)
        {
            var attributesHelp = prefix == "html:" ? " If you want to set html attributes make sure to register the style for control that supports them, for example using Styles.Register<HtmlGenericControl>(...)..." : "";
            throw new Exception($"Control {sb.ControlType.Name} does not have any property group with prefix '{prefix}'." + attributesHelp);
        }
        if (group.Length > 1)
            throw new Exception($"Control {sb.ControlType.Name} has ambiguous property groups with prefix '{prefix}'");

        return group.Single().GetDotvvmProperty(member);
    }

    /// <summary> Sets property group member of the control. For example SetPropertyGroupMember("Param-", "Abcd", 1) would set the Abcd parameter on RouteLink. </summary>
    public static T SetPropertyGroupMember<T>(
        this T sb,
        string prefix,
        string memberName,
        object? value,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        where T: IStyleBuilder =>
        sb.SetDotvvmProperty(sb.GetPropertyGroup(prefix, memberName), value, options);

    /// <summary> Sets property group member of the control. For example SetPropertyGroupMember("Param-", "Abcd", 1) would set the Abcd parameter on RouteLink. </summary>
    public static IStyleBuilder<T> SetPropertyGroupMember<T>(
        this IStyleBuilder<T> sb,
        string prefix,
        string memberName,
        Func<IStyleMatchContext<T>, object?> value,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite) =>
        sb.SetDotvvmProperty(sb.GetPropertyGroup(prefix, memberName), value, options);

    /// <summary> Sets the controls children. </summary>
    public static T SetContent<T, TControl>(
        this T sb,
        TControl prototypeControl,
        Action<StyleBuilder<TControl>>? styleBuilder = null,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        where T: IStyleBuilder<DotvvmControl>
        where TControl: DotvvmBindableObject
    {
        if (prototypeControl is null) throw new ArgumentNullException(nameof(prototypeControl));
        var innerControlStyleBuilder = new StyleBuilder<TControl>(null, false);
        styleBuilder?.Invoke(innerControlStyleBuilder);

        return sb.AddApplicator(
            new ChildrenStyleApplicator(
                options,
                new FunctionOrValue<IStyleMatchContext, IEnumerable<DotvvmBindableObject>>.ValueCase(new [] { prototypeControl }),
                innerControlStyleBuilder.GetStyle())
        );
    }
    /// <summary> Appends a child control. </summary>
    public static T AppendContent<T, TControl>(
        this T sb,
        TControl prototypeControl,
        Action<StyleBuilder<TControl>>? styleBuilder = null)
        where T: IStyleBuilder<DotvvmControl>
        where TControl: DotvvmBindableObject =>
        sb.SetContent(prototypeControl, styleBuilder, StyleOverrideOptions.Append);
    /// <summary> Prepends a child control. </summary>
    public static T PrependContent<T, TControl>(
        this T sb,
        TControl prototypeControl,
        Action<StyleBuilder<TControl>>? styleBuilder = null)
        where T: IStyleBuilder<DotvvmControl>
        where TControl: DotvvmBindableObject =>
        sb.SetContent(prototypeControl, styleBuilder, StyleOverrideOptions.Prepend);

    /// <summary> Sets the controls children. </summary>
    public static IStyleBuilder<T> SetContent<T, TControl>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, TControl> prototypeControl,
        Action<StyleBuilder<TControl>>? styleBuilder = null,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        where T: DotvvmControl
        where TControl: DotvvmBindableObject
    {
        var innerControlStyleBuilder = new StyleBuilder<TControl>(null, false);
        styleBuilder?.Invoke(innerControlStyleBuilder);

        return sb.AddApplicator(
            new ChildrenStyleApplicator(
                options,
                new FunctionOrValue<IStyleMatchContext, IEnumerable<DotvvmBindableObject>>.FunctionCase(c => new [] { prototypeControl(c.Cast<T>()) }),
                innerControlStyleBuilder.GetStyle())
        );
    }
    /// <summary> Appends a child control. </summary>
    public static IStyleBuilder<T> AppendContent<T, TControl>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, TControl> prototypeControl,
        Action<StyleBuilder<TControl>>? styleBuilder = null)
        where T: DotvvmControl
        where TControl: DotvvmBindableObject =>
        sb.SetContent(prototypeControl, styleBuilder, StyleOverrideOptions.Append);
    /// <summary> Prepends a child control. </summary>
    public static IStyleBuilder<T> PrependContent<T, TControl>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, TControl> prototypeControl,
        Action<StyleBuilder<TControl>>? styleBuilder = null)
        where T: DotvvmControl
        where TControl: DotvvmBindableObject =>
        sb.SetContent(prototypeControl, styleBuilder, StyleOverrideOptions.Prepend);

    public static T AddApplicator<T>(
        this T sb,
        IStyleApplicator applicator)
        where T: IStyleBuilder
    {
        sb.AddApplicatorImpl(applicator);
        return sb;
    }

    /// <summary> Applies this style only to controls matching this condition. When multiple conditions are specified, all are combined using the AND operator. </summary>
    [Obsolete("Usage of AddCondition should be preferred. The methods are equivalent, but the name WithCondition falsely indicates that new builder with the condition is created.")]
    public static T WithCondition<T>(this T sb, Func<IStyleMatchContext, bool> condition)
        where T: IStyleBuilder =>
        sb.AddCondition(condition);
    /// <summary> Applies this style only to controls matching this condition. When multiple conditions are specified, all are combined using the AND operator. </summary>
    public static T AddCondition<T>(this T sb, Func<IStyleMatchContext, bool> condition)
        where T: IStyleBuilder
    {
        sb.AddConditionImpl(condition);
        return sb;
    }
    /// <summary> Applies this style only to controls matching this condition. When multiple conditions are specified, all are combined using the AND operator. </summary>
    public static IStyleBuilder<TControl> AddCondition<TControl>(this IStyleBuilder<TControl> sb, Func<IStyleMatchContext<TControl>, bool> condition)
    {
        sb.AddConditionImpl(condition);
        return sb;
    }
}
