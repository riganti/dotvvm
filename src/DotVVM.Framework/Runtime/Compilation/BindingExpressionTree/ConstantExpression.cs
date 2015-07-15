using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.BindingExpressionTree
{
    public class ConstantExpression: BindingExpressionNode
    {
        public object Value { get; set; }

        public override bool IsViewModel => false;

        public ConstantExpression(object value)
        {
            Value = value;
            Type = value.GetType();
        }

        public override void Accept(BindingExpressionTreeVisitor visitor)
        {
            visitor.VisitConstant(this);
        }

        public override void AcceptChildred(BindingExpressionTreeVisitor visitor)
        {
        }
    }
}
