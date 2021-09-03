using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using System.Reflection;
using System.IO;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Compilation.Styles
{
    public interface IStyleMatchContext
    {
        IStyleMatchContext? Parent { get; }
        ResolvedControl Control { get; }
        DotvvmConfiguration Configuration { get; }
    }
    public interface IStyleMatchContext<out TControl> : IStyleMatchContext
    {
    }

    public class StyleMatchContext<T> : IStyleMatchContext<T>
    {
        public StyleMatchContext(IStyleMatchContext? parent, ResolvedControl control, DotvvmConfiguration configuration)
        {
            if (!typeof(T).IsAssignableFrom(control.Metadata.Type))
                throw new ArgumentException($"Control {control.Metadata.Type} is not assignable to type {typeof(T)}", nameof(control));
            Parent = parent;
            Control = control;
            Configuration = configuration;
        }

        public IStyleMatchContext? Parent { get; }
        public ResolvedControl Control { get; }

        public IEnumerable<IStyleMatchContext> Ancestors => this.GetAncestors();

        public DotvvmConfiguration Configuration { get; }
    }
}
