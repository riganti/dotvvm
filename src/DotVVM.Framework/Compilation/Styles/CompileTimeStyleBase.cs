#nullable enable
using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Styles
{
    internal abstract class CompileTimeStyleBase : IStyle
    {
        readonly List<IStyleApplicator> applicators = new List<IStyleApplicator>();
        public Type ControlType { get; }
        public bool ExactTypeMatch { get; }
        protected CompileTimeStyleBase(Type controlType, bool exactTypeMatch)
        {
            ControlType = controlType;
            ExactTypeMatch = exactTypeMatch;
        }
        public IStyleApplicator Applicator => MonoidStyleApplicator.Combine(applicators);

        public abstract bool Matches(IStyleMatchContext context);
        public void AddApplicator(IStyleApplicator a) => applicators.Add(a);


        public override string ToString() => $"Style for {(ExactTypeMatch ? "exactly " : "")}{ControlType}: {Applicator}";
    }
}
