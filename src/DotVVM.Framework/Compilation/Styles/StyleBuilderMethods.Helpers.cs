#nullable enable
using System;
using System.Collections.Generic;
using DotVVM.Framework.Utils;
using System.Reflection;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Compilation.Styles;

public static partial class StyleBuilderExtensionMethods
{
    public static T AddPostbackHandler<T>(
        this T sb,
        PostBackHandler handler)
        where T: IStyleBuilder =>
        sb.SetControlProperty(PostBack.ConcurrencyQueueSettingsProperty, handler);

    public static IStyleBuilder<T> AddPostbackHandler<T>(
        this IStyleBuilder<T> sb,
        Func<IStyleMatchContext<T>, PostBackHandler> handler)
        where T: DotvvmBindableObject =>
        sb.SetDotvvmProperty(PostBack.ConcurrencyQueueSettingsProperty, handler);


    public static T AddRequiredResource<T>(
        this T sb,
        params string[] resources)
        where T: IStyleBuilder =>
        sb.SetDotvvmProperty(Styles.RequiredResourcesProperty, resources, StyleOverrideOptions.Append);
}
