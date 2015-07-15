using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation.BindingExpressionTree;
using System.Reflection;

namespace DotVVM.Framework.Runtime.Compilation.JavascriptCompilation
{
    public class JavascriptCompilingVisitor : BindingExpressionTreeVisitor
    {
        public IDictionary<MethodInfo, string> MethodCompilers { get; set; }

        private Stack<List<string>> valuesStack = new Stack<List<string>>();
        private List<string> values = new List<string>();

        private void PushValues()
        {
            valuesStack.Push(values);
            values = new List<string>();
        }

        private List<string> PopValues()
        {
            return values = valuesStack.Pop();
        }

        public override void VisitConstant(ConstantExpression expression)
        {
            values.Add(JavascriptCompilationHelper.CompileConstant(expression.Value));
        }

        public override void VisitMethodInvocation(MethodInvocation expression)
        {
            PushValues();
            string thisExpression = null;
            if (expression.Expression != null)
            {
                expression.Expression.Accept(this);
                thisExpression = values.First();
                values.Clear();
            }
            foreach (var arg in expression.Arguments)
            {
                arg.Accept(this);
            }
            var methodCompiler = MethodCompilers[expression.Method];
            var result = string.Format(methodCompiler, Enumerable.Concat(new[] { thisExpression }, values).ToArray());
            PopValues();
            values.Add(result);
        }

        public override void VisitViewModelProperty(ViewModelPropertyAccess expression)
        {
            PushValues();
            expression.Expression.Accept(this);
            var on = values.Single();
            PopValues();
            // TODO: null propagation
            values.Add(expression + "()." + expression.Property.Name);
        }
    }
}
