using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class DataContextStack
    {
        public DataContextStack Parent { get; set; }
        public Type DataContextType { get; set; }
        //public Expression ReplaceExpression { get; set; }
        //public List<Type> Parameters { get; set; }
        public Type RootControlType { get; set; }

        public DataContextStack(Type type, DataContextStack parent = null)
        {
            Parent = parent;
            DataContextType = type;
            RootControlType = parent?.RootControlType;
            //if (parent == null) Parameters = new List<Type>();
            //else Parameters = parent.Parameters;
        }

        //public DataContextStack(Expression expression, DataContextStack parent = null)
        //    : this(expression.Type, parent)
        //{
        //    ReplaceExpression = expression;
        //}

        public IEnumerable<Type> Enumerable()
        {
            var c = this;
            while (c != null)
            {
                yield return c.DataContextType;
                c = c.Parent;
            }
        }

        public IEnumerable<Type> Parents()
        {
            var c = Parent;
            while(c != null)
            {
                yield return c.DataContextType;
                c = c.Parent;
            }
        }

        //public ParameterExpression GetNextParameter(Type type)
        //{
        //    var i = Parameters.Count;
        //    Parameters.Add(type);
        //    return Expression.Parameter(type, "param_" + i);
        //}
    }
}
