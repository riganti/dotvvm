using System;
using System.Collections;
using System.Linq;
using DotVVM.Framework.Binding;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class CompileTimeTypeConstraint
    {
        private readonly Func<Type, bool> condition;

        public CompileTimeTypeConstraint(Func<Type, bool> condition)
        {
            this.condition = condition;
        }



        public bool CheckType(Type type)
        {
            return condition(type);
        }


        public static readonly CompileTimeTypeConstraint Any = new CompileTimeTypeConstraint(t => true);

        public static readonly CompileTimeTypeConstraint Boolean = new CompileTimeTypeConstraint(t => t == typeof(bool) || t == typeof(bool?));

        public static readonly CompileTimeTypeConstraint Numeric = new CompileTimeTypeConstraint(t => NumberUtils.IsNumericType(t) || NumberUtils.IsNullableNumericType(t));

        public static readonly CompileTimeTypeConstraint Array = new CompileTimeTypeConstraint(t => t.IsArray || t.IsInstanceOfType(typeof(IList)));



        public static CompileTimeTypeConstraint AnyOf(params CompileTimeTypeConstraint[] constraints)
        {
            return new CompileTimeTypeConstraint(t => constraints.Any(c => c.CheckType(t)));
        }

        public static CompileTimeTypeConstraint ExactType(Type type)
        {
            return new CompileTimeTypeConstraint(t => t == type);
        }
    }
}