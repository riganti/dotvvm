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
        public static readonly JsTreeRole<JsExpression> Argument = new JsTreeRole<JsExpression>("Argument");
        public static readonly JsTreeRole<JsExpression> Expression = new JsTreeRole<JsExpression>("Expression");
        public static readonly JsTreeRole<JsExpression> TargetExpression = new JsTreeRole<JsExpression>("Target");
        public readonly static JsTreeRole<JsExpression> Condition = new JsTreeRole<JsExpression>("Condition");
    }
}
