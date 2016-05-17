using System;

namespace DotVVM.Framework.Compilation.Styles
{
    public interface IStyle
    {
        bool Matches(StyleMatchContext currentControl);
        IStyleApplicator Applicator { get; }
        Type ControlType { get; }
        bool ExactTypeMatch { get; }
    }
}
