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

namespace DotVVM.Framework.Compilation.Styles
{
    // not generic interface
    public interface IStyleBuilder
    {
        void AddConditionImpl(Func<IStyleMatchContext, bool> condition);
        void AddApplicatorImpl(IStyleApplicator applicator);
        IStyle GetStyle();
        Type ControlType { get; }
    }

    public interface IStyleBuilder<out TControl> : IStyleBuilder
    {
        void AddConditionImpl(Func<IStyleMatchContext<TControl>, bool> condition);
    }

    public class StyleBuilder<T> : IStyleBuilder<T>
    {
        private Style style;

        public Type ControlType => style.ControlType;

        public StyleBuilder(Func<StyleMatchContext<T>, bool>? matcher, bool allowDerived)
        {
            style = new Style(!allowDerived, matcher);
        }

        public IStyle GetStyle() => style;

        public void AddConditionImpl(Func<IStyleMatchContext<T>, bool> condition)
        {
            var oldMatcher = style.Matcher;
            if (oldMatcher is null)
                style.Matcher = condition;
            else
                style.Matcher = m => oldMatcher(m) && condition(m);
        }

        public void AddApplicatorImpl(IStyleApplicator applicator)
        {
            style.AddApplicator(applicator);
        }

        void IStyleBuilder.AddConditionImpl(Func<IStyleMatchContext, bool> condition) => AddConditionImpl(condition);

        class Style : CompileTimeStyleBase
        {
            public Style(bool exactTypeMatch, Func<StyleMatchContext<T>, bool>? matcher)
                : base(typeof(T), exactTypeMatch)
            {
                Matcher = matcher;
            }

            public Func<StyleMatchContext<T>, bool>? Matcher { get; set; }

            public override bool Matches(IStyleMatchContext context)
            {
                return Matcher != null ? Matcher(new StyleMatchContext<T>(context.Parent, context.Control, context.Configuration)) : true;
            }
        }
    }
}
