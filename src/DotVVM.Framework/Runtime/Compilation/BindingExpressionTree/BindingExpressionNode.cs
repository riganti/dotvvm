using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.BindingExpressionTree
{
    public abstract class BindingExpressionNode
    {
        public abstract bool IsViewModel { get; }
        public virtual Type Type { get; set; }
        public abstract void Accept(BindingExpressionTreeVisitor visitor);
        public abstract void AcceptChildred(BindingExpressionTreeVisitor visitor);
    }
}
