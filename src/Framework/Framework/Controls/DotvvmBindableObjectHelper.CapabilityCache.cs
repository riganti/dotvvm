using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    public static partial class DotvvmBindableObjectHelper
    {
        internal class CapabilityPropertyCache<T>
        {
            internal readonly static DotvvmCapabilityProperty? HtmlCapabilityProperty =
                DotvvmCapabilityProperty.Find(typeof(T), typeof(HtmlCapability));
            internal readonly static DotvvmPropertyGroup? HtmlAttributes =
                HtmlCapabilityProperty?.FindPropertyGroup(nameof(HtmlCapability.Attributes));
            internal readonly static DotvvmPropertyGroup? CssClasses =
                HtmlCapabilityProperty?.FindPropertyGroup(nameof(HtmlCapability.CssClasses));
            internal readonly static DotvvmPropertyGroup? CssStyles =
                HtmlCapabilityProperty?.FindPropertyGroup(nameof(HtmlCapability.CssStyles));
            internal readonly static DotvvmProperty? Visible =
                HtmlCapabilityProperty?.FindProperty(nameof(HtmlCapability.Visible));
        }

        /// <summary> Returns a mutable dictionary of html attributes defined in <see cref="HtmlCapability.Attributes" /> </summary>
        public static VirtualPropertyGroupDictionary<object?> GetHtmlAttributesDictionary<T>(this T control)
            where T: IObjectWithCapability<HtmlCapability>
        {
            var c = CapabilityPropertyCache<T>.HtmlAttributes;
            if (c is null)
                throw new ArgumentException($"Type {typeof(T).FullName} does not have HtmlCapability.Attributes with proper mapping.");
            return new VirtualPropertyGroupDictionary<object?>(control.Self, c);
        }

        /// <summary> Returns a mutable dictionary of css classes defined in the <see cref="HtmlCapability.CssClasses" /> </summary>
        public static VirtualPropertyGroupDictionary<bool> GetCssClassesDictionary<T>(this T control)
            where T: IObjectWithCapability<HtmlCapability>
        {
            var c = CapabilityPropertyCache<T>.CssClasses;
            if (c is null)
                throw new ArgumentException($"Type {typeof(T).FullName} does not have HtmlCapability.CssClasses with proper mapping.");
            return new VirtualPropertyGroupDictionary<bool>(control.Self, c);
        }

        /// <summary> Returns a mutable dictionary of css styles defined in the <see cref="HtmlCapability.CssStyles" /> </summary>
        public static VirtualPropertyGroupDictionary<object?> GetCssStylesDictionary<T>(this T control)
            where T: IObjectWithCapability<HtmlCapability>
        {
            var c = CapabilityPropertyCache<T>.CssStyles;
            if (c is null)
                throw new ArgumentException($"Type {typeof(T).FullName} does not have HtmlCapability.CssStyles with proper mapping.");
            return new VirtualPropertyGroupDictionary<object?>(control.Self, c);
        }

        /// <summary> Little hack to convert <see cref="IObjectWithCapability{HtmlCapability}" /> to IControlWithHtmlAttributes, so we can share code paths </summary>
        internal static ControlAsObjectWithAttributes AsObjectWithHtmlAttributes<T>(this T control)
            where T: IObjectWithCapability<HtmlCapability>
        {
            return new ControlAsObjectWithAttributes(control.GetHtmlAttributesDictionary(), control.Self);
        }
        internal struct ControlAsObjectWithAttributes : IControlWithHtmlAttributes, IObjectWithCapability<HtmlCapability>
        {
            public ControlAsObjectWithAttributes(VirtualPropertyGroupDictionary<object?> attributes, DotvvmBindableObject self)
            {
                Attributes = attributes;
                Self = self;
            }

            public VirtualPropertyGroupDictionary<object?> Attributes { get; }

            public DotvvmBindableObject Self { get; }
        }
    }
}
