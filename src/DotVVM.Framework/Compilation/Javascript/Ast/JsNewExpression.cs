using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsNewExpression: JsExpression
    {
		public JsExpression Target
		{
			get { return GetChildByRole(JsTreeRoles.TargetExpression); }
			set { SetChildByRole(JsTreeRoles.TargetExpression, value); }
		}
		public JsNodeCollection<JsExpression> Arguments
		{
			get { return GetChildrenByRole<JsExpression>(JsTreeRoles.Argument); }
		}

		public JsNewExpression() { }

		public JsNewExpression(JsExpression target, IEnumerable<JsExpression> arguments)
		{
			AddChild(target, JsTreeRoles.TargetExpression);
			if (arguments != null)
			{
				foreach (var arg in arguments)
				{
					AddChild(arg, JsTreeRoles.Argument);
				}
			}
		}

		public JsNewExpression(JsExpression target, params JsExpression[] arguments) : this(target, (IEnumerable<JsExpression>)arguments) { }
		public JsNewExpression(string name, params JsExpression[] arguments) : this(new JsIdentifierExpression(name), (IEnumerable<JsExpression>)arguments) { }
		public JsNewExpression(string name, IEnumerable<JsExpression> arguments) : this(new JsIdentifierExpression(name), arguments) { }

		public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitNewExpression(this);
	}
}
