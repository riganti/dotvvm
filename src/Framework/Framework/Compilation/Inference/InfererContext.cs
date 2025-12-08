using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.Binding;

namespace DotVVM.Framework.Compilation.Inference
{
    internal class InfererContext
    {
        public MethodGroupExpression? Target { get; set; }
        public List<MethodBase> Candidates { get; set; }
        public Expression[] Arguments { get; set; }
        public Dictionary<Type, Type> Generics { get; set; }
        public int CurrentArgumentIndex { get; set; }
        public bool IsExtensionCall { get; set; }

        public InfererContext(MethodGroupExpression? target, int argsCount)
        {
            this.Target = target;
            this.Candidates = target?.Candidates?.ToList<MethodBase>() ?? [];
            this.Arguments = new Expression[argsCount];
            this.Generics = new();
        }

        public InfererContext(List<MethodBase> candidates, int argsCount)
        {
            this.Candidates = candidates;
            this.Arguments = new Expression[argsCount];
            this.Generics = new();
        }
    }
}
