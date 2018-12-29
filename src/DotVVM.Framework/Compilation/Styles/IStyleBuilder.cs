using System;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Compilation.Styles
{
    // not generic interface
    public interface IStyleBuilder
    {
        IStyleBuilder SetDotvvmProperty(DotvvmProperty property, object value);
        IStyleBuilder WithCondition(Func<StyleMatchContext, bool> condition);
        IStyle GetStyle();
    }
}
