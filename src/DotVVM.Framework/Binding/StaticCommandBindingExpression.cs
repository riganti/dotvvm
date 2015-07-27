using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser.Translation;
using System.Reflection;
using System.Linq.Expressions;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Binding
{
    [BindingCompilationRequirements(Expression = BindingCompilationRequirementType.StronglyRequire, OriginalString = BindingCompilationRequirementType.StronglyRequire)]
    public class StaticCommandBindingExpression : BindingExpression
    {
        private static ExpressionTranslator translator = new ExpressionTranslator();

        public StaticCommandBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        { } 
        public override object Evaluate(DotvvmBindableControl control, DotvvmProperty property)
        {
            throw new NotImplementedException();
        }

        public override string TranslateToClientScript(DotvvmBindableControl control, DotvvmProperty property)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetArgumentPaths()
        {
            // TODO: replace roslyn runtime parsing with precompiled
            var expression = ExpressionTranslator.ParseExpression(OriginalString) as InvocationExpressionSyntax;
            if (expression == null) throw new Exception("static command must be invocation expression");
            var arguments = expression.ArgumentList.Arguments;
            return arguments.Select(argument => translator.TranslateExpression(argument.Expression));
        }

        public string GetMethodName(DotvvmBindableControl control)
        {
            //var expression = ExpressionTranslator.ParseExpression(OriginalString) as InvocationExpressionSyntax;
            //if (expression == null) throw new Exception("static command must be invocation expression");
            //var accessor = expression.Expression;
            //if (accessor.Kind() == SyntaxKind.IdentifierName)
            //{
            //    var name = (accessor as IdentifierNameSyntax).Identifier.ToString();
            //    // simple method name without type name
            //    var dataContextType = control.DataContext.GetType();
            //    var method = dataContextType.GetMethod(name);
            //    if (method == null) throw new Exception("method not found on type");
            //    return method.DeclaringType.AssemblyQualifiedName + "." + method.Name;
            //}
            //else if (accessor.Kind() == SyntaxKind.SimpleMemberAccessExpression)
            //{
            //    return accessor.ToFullString();
            //}
            //throw new NotSupportedException();
            var methodInvocation = this.ExpressionTree as MethodCallExpression;
            if (methodInvocation == null) throw new NotSupportedException("static command must be method call expression");
            return methodInvocation.Method.DeclaringType.AssemblyQualifiedName + "." + methodInvocation.Method.Name;
        }
    }
}
