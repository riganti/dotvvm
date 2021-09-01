using System;

namespace DotVVM.Framework.Compilation.Styles
{
    public interface IStyle
    {
        bool Matches(IStyleMatchContext context);
        IStyleApplicator Applicator { get; }
        Type ControlType { get; }
        bool ExactTypeMatch { get; }
    }
}
