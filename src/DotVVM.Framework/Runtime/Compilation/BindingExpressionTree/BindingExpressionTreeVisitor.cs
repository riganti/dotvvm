using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.BindingExpressionTree
{
    public class BindingExpressionTreeVisitor
    {
        public virtual void VisitConstant(ConstantExpression expression)
        {
            DefaultVisit(expression);
        }

        public virtual void VisitViewModelProperty(ViewModelPropertyAccess expression)
        {
            DefaultVisit(expression);
        }

        public virtual void VisitMethodInvocation(MethodInvocation expression)
        {
            DefaultVisit(expression);
        }

        public virtual void VisitKeyword(KeywordExpression expression)
        {
            DefaultVisit(expression);
        }

        public virtual void DefaultVisit(BindingExpressionNode node)
        {
            node.AcceptChildred(this);
        }
    }
}
