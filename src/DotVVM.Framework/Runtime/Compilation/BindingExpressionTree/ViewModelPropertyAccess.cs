using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.BindingExpressionTree
{
    public class ViewModelPropertyAccess: BindingExpressionNode
    {
        public BindingExpressionNode Expression { get; set; }

        public override bool IsViewModel => typeof(IEnumerable).IsAssignableFrom(Expression.Type);

        public PropertyInfo Property { get; set; }

        public ViewModelPropertyAccess(BindingExpressionNode expression, PropertyInfo property)
        {
            this.Expression = expression;
            this.Property = property;
            this.Type = property.PropertyType;
        }

        public override void Accept(BindingExpressionTreeVisitor visitor)
        {
            visitor.VisitViewModelProperty(this);
        }

        public override void AcceptChildred(BindingExpressionTreeVisitor visitor)
        {
            Expression.Accept(visitor);
        }
    }
}
