using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Compilation
{
    public class EmitterMethodInfo
    {

        public List<StatementSyntax> Statements { get; set; }

        public string Name { get; set; }

        public ParameterListSyntax Parameters { get; set; }

        public TypeSyntax ReturnType { get; set; }

        public EmitterMethodInfo(TypeSyntax returnType, params ParameterSyntax[] parameters)
        {
            Parameters = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));
            ReturnType = returnType;
            Statements = new List<StatementSyntax>();
        }
    }
}