using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Binding
{
    //public class GenericTypeExpression : Expression
    //{
    //    public string IdentifierName { get; }
    //    public Type[] TypeArguments { get; }     

    //    public override Type Type => throw new Not 
       

    //    public override bool CanReduce
    //    {
    //        get
    //        {
    //            return Type != null;
    //        }
    //    }


    //    public GenericTypeExpression(string identifierName, Type[] typeArguments)
    //    {
    //        IdentifierName = identifierName;
    //        TypeArguments = typeArguments;
    //    }

    //    public override Expression Reduce()
    //    {
    //        return new StaticClassIdentifierExpression(Type);
    //    }

    //    public override string ToString()
    //    {
    //        return $"{Target}.{IdentifierName}<{string.Join(" ,", TypeArguments.Select(t=> t.FullName))}>";
    //    }
    //}
}
