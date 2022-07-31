using System;
using System.Collections.Generic;
using DotVVM.Framework.Utils;
using System.Reflection;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Compilation.Styles;

public static partial class StyleBuilderExtensionMethods
{
    /// <summary> Adds a new postback handler to the PostBack.Handlers property </summary>
    public static T AddPostbackHandler<T>(
        this T sb,
        PostBackHandler handler)
        where T: IStyleBuilder =>
        sb.SetControlProperty(PostBack.HandlersProperty, handler, options: StyleOverrideOptions.Append);

    /// <summary> Adds a new postback handler to the PostBack.Handlers property </summary>
    public static IStyleBuilder<T> AddPostbackHandler<T>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, PostBackHandler> handler)
        where T: DotvvmBindableObject =>
        sb.SetDotvvmProperty(PostBack.HandlersProperty, handler, StyleOverrideOptions.Append);


    /// <summary> Requests a resource to be included if this control is in the page. </summary>
    public static T AddRequiredResource<T>(
        this T sb,
        params string[] resources)
        where T: IStyleBuilder =>
        sb.SetDotvvmProperty(Styles.RequiredResourcesProperty, resources, StyleOverrideOptions.Append);

    /// <summary> Replaces the matching controls with a new control while copying all properties to the new one.</summary>
    public static T ReplaceWith<T, TControl>(
        this T sb,
        TControl newControl,
        Action<IStyleBuilder<TControl>>? styleBuilder = null,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        where T: IStyleBuilder
        where TControl: DotvvmBindableObject =>
        sb.SetControlProperty(
            Styles.ReplaceWithProperty,
            newControl,
            styleBuilder,
            options
        );
    /// <summary> Replaces the matching controls with a new control while copying all properties to the new one.</summary>
    public static IStyleBuilder<T> ReplaceWith<T>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, DotvvmBindableObject> handler,
        StyleOverrideOptions options = StyleOverrideOptions.Overwrite) =>
        sb.SetDotvvmProperty(Styles.ReplaceWithProperty, handler, options);

    /// <summary> Wraps the matching controls with a new control - places the new control instead of the original control which is moved inside the wrapper control </summary>
    public static T WrapWith<T, TControl>(
        this T sb,
        TControl wrapperControl,
        Action<IStyleBuilder<TControl>>? styleBuilder = null,
        StyleOverrideOptions options = StyleOverrideOptions.Append)
        where T: IStyleBuilder
        where TControl: DotvvmBindableObject =>
        sb.SetControlProperty(
            Styles.WrappersProperty,
            wrapperControl,
            styleBuilder,
            options
        );
    /// <summary> Wraps the matching controls with a new control - places the new control instead of the original control which is moved inside the wrapper control </summary>
    public static IStyleBuilder<T> WrapWith<T>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, DotvvmBindableObject> handler,
        StyleOverrideOptions options = StyleOverrideOptions.Append) =>
        sb.SetDotvvmProperty(Styles.WrappersProperty, handler, options);

    /// <summary> Adds a new control bellow the matched control. </summary>
    public static T Append<T, TControl>(
        this T sb,
        TControl control,
        Action<IStyleBuilder<TControl>>? styleBuilder = null,
        StyleOverrideOptions options = StyleOverrideOptions.Append)
        where T: IStyleBuilder
        where TControl: DotvvmBindableObject =>
        sb.SetControlProperty(
            Styles.AppendProperty,
            control,
            styleBuilder,
            options
        );
    /// <summary> Adds a new control bellow the matched control. </summary>
    public static IStyleBuilder<T> Append<T>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, DotvvmBindableObject> handler,
        StyleOverrideOptions options = StyleOverrideOptions.Append) =>
        sb.SetDotvvmProperty(Styles.AppendProperty, handler, options);

    /// <summary> Adds a new control above the matched control. </summary>
    public static T Prepend<T, TControl>(
        this T sb,
        TControl control,
        Action<IStyleBuilder<TControl>>? styleBuilder = null,
        StyleOverrideOptions options = StyleOverrideOptions.Append)
        where T: IStyleBuilder
        where TControl: DotvvmBindableObject =>
        sb.SetControlProperty(
            Styles.PrependProperty,
            control,
            styleBuilder,
            options
        );
    /// <summary> Adds a new control above the matched control. </summary>
    public static IStyleBuilder<T> Prepend<T, TControl>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, DotvvmBindableObject> handler,
        StyleOverrideOptions options = StyleOverrideOptions.Append) =>
        sb.SetDotvvmProperty(Styles.PrependProperty, handler, options);

    /// <summary> The control will be removed from the control tree if <paramref name="condition"/> is true. Note that all appended, prepended and wrapper controls will be preserved. </summary>
    public static IStyleBuilder<T> Remove<T>(
        this IStyleBuilder<T> sb,
        bool condition = true,
        StyleOverrideOptions options = StyleOverrideOptions.Append) =>
        sb.SetDotvvmProperty(Styles.RemoveProperty, condition, options);

    /// <summary> The control will be removed from the control tree if <paramref name="condition"/> is true. Note that all appended, prepended and wrapper controls will be preserved. </summary>
    public static IStyleBuilder<T> Remove<T>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, bool> condition,
        StyleOverrideOptions options = StyleOverrideOptions.Append) =>
        sb.SetDotvvmProperty(Styles.RemoveProperty, condition, options);
}
