using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Utils;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using System.Linq;

namespace DotVVM.Framework.Compilation.Styles
{
    // not generic interface
    public interface IStyleBuilder
    {
        IStyleBuilder SetDotvvmProperty(DotvvmProperty property, object value);
        IStyleBuilder WithCondition(Func<StyleMatchContext, bool> condition);
        IStyle GetStyle();
    }

    public class StyleBuilder<T> : IStyleBuilder
    {
        private Style style;

        public StyleBuilder(Func<StyleMatchContext, bool> matcher, bool allowDerived)
        {
            style = new Style(!allowDerived, matcher);
        }

        private static DotvvmProperty GetProperty(string name)
        {
            var field = typeof(T).GetField(name + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            return field.GetValue(null) as DotvvmProperty;
        }

        public StyleBuilder<T> SetProperty<TProperty>(Expression<Func<T, TProperty>> property, TProperty value)
        {
            var propertyName = ReflectionUtils.GetMemberFromExpression(property.Body).Name;
            return SetDotvvmProperty(GetProperty(propertyName), value);
        }

        public StyleBuilder<T> SetDotvvmProperty(ResolvedPropertySetter setter, StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        {
            style.SetProperties[setter.Property] = new CompileTimeStyleBase.PropertyInsertionInfo(setter, options);
            return this;
        }

        public StyleBuilder<T> SetDotvvmProperty(DotvvmProperty property, object value, StyleOverrideOptions options = StyleOverrideOptions.Overwrite) =>
            SetDotvvmProperty(new ResolvedPropertyValue(property, value), options);

        public StyleBuilder<T> SetAttribute(string attribute, object value, StyleOverrideOptions options = StyleOverrideOptions.Ignore) =>
            SetPropertyGroupMember("", attribute, value, options);

        public StyleBuilder<T> SetPropertyGroupMember(string prefix, string memberName, object value, StyleOverrideOptions options = StyleOverrideOptions.Overwrite)
        {
            var prop = DotvvmPropertyGroup.GetPropertyGroups(typeof(T)).Single(p => p.Prefixes.Contains(prefix));
            return SetDotvvmProperty(prop.GetDotvvmProperty(memberName), value, options);
        }

        public StyleBuilder<T> WithCondition(Func<StyleMatchContext, bool> condition)
        {
            var oldMatcher = style.Matcher;
            if (style.Matcher == null)
                style.Matcher = condition;
            else
                style.Matcher = m => oldMatcher(m) && condition(m);
            return this;
        }

        public IStyle GetStyle()
        {
            return style;
        }

        IStyleBuilder IStyleBuilder.SetDotvvmProperty(DotvvmProperty property, object value)
            => SetDotvvmProperty(property, value);

        IStyleBuilder IStyleBuilder.WithCondition(Func<StyleMatchContext, bool> condition)
            => WithCondition(condition);

        public class Style : CompileTimeStyleBase
        {
            public Style(bool exactTypeMatch = false, Func<StyleMatchContext, bool> matcher = null)
            {
                Matcher = matcher;
                ExactTypeMatch = exactTypeMatch;
            }

            public override Type ControlType => typeof(T);

            public Func<StyleMatchContext, bool> Matcher { get; set; }
            public override bool Matches(StyleMatchContext matchInfo)
            {
                return Matcher != null ? Matcher(matchInfo) : true;
            }
        }
    }

    public enum StyleOverrideOptions
    {
        Ignore,
        Overwrite,
        Append
    }
}
