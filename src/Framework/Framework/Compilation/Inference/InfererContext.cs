using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Binding;

namespace DotVVM.Framework.Compilation.Inference
{
    internal class InfererContext
    {
        public MethodGroupExpression? Target { get; set; }
        public Expression[] Arguments { get; set; }
        public Dictionary<string, Type> Generics { get; set; }
        public int CurrentArgumentIndex { get; set; }
        public bool IsExtensionCall { get; set; }

        public InfererContext(MethodGroupExpression? target, int argsCount)
        {
            this.Target = target;
            this.Arguments = new Expression[argsCount];
            this.Generics = new Dictionary<string, Type>();
        }
    }
}
