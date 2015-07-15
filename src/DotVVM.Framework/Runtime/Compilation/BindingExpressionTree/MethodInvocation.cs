using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.BindingExpressionTree
{
    public class MethodInvocation: BindingExpressionNode
    {

        public BindingExpressionNode Expression { get; set; }
        public BindingExpressionNode[] Arguments { get; set; }
        public MethodInfo Method { get; set; }

        public override bool IsViewModel => false;

        public MethodInvocation(BindingExpressionNode expression, MethodInfo method, params BindingExpressionNode[] arguments)
        {
            Expression = expression;
            Method = method;
            Type = method.ReturnType;
            Arguments = arguments;
        }

        public override void Accept(BindingExpressionTreeVisitor visitor)
        {
            visitor.VisitMethodInvocation(this);
        }

        public override void AcceptChildred(BindingExpressionTreeVisitor visitor)
        {
            Expression.Accept(visitor);
            foreach (var arg in Arguments) arg.Accept(visitor);
        }
    }
}
