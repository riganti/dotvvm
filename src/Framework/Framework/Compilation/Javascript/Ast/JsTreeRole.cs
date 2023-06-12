using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsTreeRole<T> : JsTreeRole
    {
        public JsTreeRole(string name) : base(name) { }
        public override bool IsValid(object o) => o is T;
    }

    public abstract class JsTreeRole
    {
        public string Name { get; }
        public JsTreeRole(string name)
        {
            this.Name = name;
        }
        public abstract bool IsValid(object o);
    }

    public static class JsTreeRoles
    {
        // some pre defined constants for common roles
        public static readonly JsTreeRole<JsIdentifier> Identifier = new JsTreeRole<JsIdentifier>("Identifier");
        //public static readonly JsTreeRole<JsBlockStatement> Body = new JsTreeRole<JsBlockStatement>("Body");
        //public static readonly JsTreeRole<JsParameterDeclaration> Parameter = new JsTreeRole<JsParameterDeclaration>("Parameter");
        public static readonly JsTreeRole<JsExpression> Argument = new JsTreeRole<JsExpression>("Argument");
        //public static readonly JsTreeRole<JsAstType> Type = new JsTreeRole<JsAstType>("Type");
        public static readonly JsTreeRole<JsExpression> Expression = new JsTreeRole<JsExpression>("Expression");
        public static readonly JsTreeRole<JsExpression> TargetExpression = new JsTreeRole<JsExpression>("Target");
        public readonly static JsTreeRole<JsExpression> Condition = new JsTreeRole<JsExpression>("Condition");

        //public static readonly JsTreeRole<JsTypeParameterDeclaration> TypeParameter = new JsTreeRole<JsTypeParameterDeclaration>("TypeParameter");
        //public static readonly JsTreeRole<JsAstType> TypeArgument = new JsTreeRole<JsAstType>("TypeArgument");
        //public readonly static JsTreeRole<JsConstraint> Constraint = new JsTreeRole<JsConstraint>("Constraint");
        //public static readonly JsTreeRole<JsVariableInitializer> Variable = new JsTreeRole<JsVariableInitializer>("Variable");
        //public static readonly JsTreeRole<JsStatement> EmbeddedStatement = new JsTreeRole<JsStatement>("EmbeddedStatement");
        //public readonly static JsTreeRole<JsEntityDeclaration> TypeMemberRole = new JsTreeRole<JsEntityDeclaration>("TypeMember");
    }
}
