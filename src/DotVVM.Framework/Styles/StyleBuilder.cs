using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Styles
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
            var field = typeof(T).GetField(name + "Property", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            return field.GetValue(null) as DotvvmProperty;
        }

        public StyleBuilder<T> SetProperty<TProperty>(Expression<Func<T, TProperty>> property, TProperty value)
        {
            var propertyName = ReflectionUtils.GetPropertyNameFromExpression(property);
            return SetDotvvmProperty(GetProperty(propertyName), value);
        }

        public StyleBuilder<T> SetDotvvmProperty(DotvvmProperty property, object value)
        {
            style.SetProperties[property] = new ResolvedPropertyValue(property, value);
            return this;
        }

        public StyleBuilder<T> SetAttribute(string attribute, object value, bool append = false)
        {
            style.SetHtmlAttributes[attribute] = new CompileTimeStyleBase.HtmlAttributeInsertionInfo { value = value, append = append };
            return this;
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
                SetHtmlAttributes = new Dictionary<string, HtmlAttributeInsertionInfo>();
                SetProperties = new Dictionary<DotvvmProperty, ResolvedPropertySetter>();
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
}
