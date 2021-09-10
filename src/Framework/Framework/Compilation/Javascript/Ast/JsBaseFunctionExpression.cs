using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public abstract class JsBaseFunctionExpression: JsExpression
    {
        private bool isAsync;
        public bool IsAsync
        {
            get { return isAsync; }
            set { ThrowIfFrozen(); isAsync = value; }
        }

        public static JsTreeRole<JsIdentifier> ParametersRole = new JsTreeRole<JsIdentifier>("Parameters");
        public JsNodeCollection<JsIdentifier> Parameters => new JsNodeCollection<JsIdentifier>(this, ParametersRole);

        public static JsTreeRole<JsBlockStatement> BlockRole = new JsTreeRole<JsBlockStatement>("Block");
        public JsBlockStatement Block
        {
            get => GetChildByRole(BlockRole)!;
            set => SetChildByRole(BlockRole, value);
        }
    }
}
