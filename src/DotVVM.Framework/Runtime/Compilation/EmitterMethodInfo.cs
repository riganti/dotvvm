using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class EmitterMethodInfo
    {

        public List<StatementSyntax> Statements { get; set; }

        public string Name { get; set; }

        public int ControlIndex { get; set; }

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