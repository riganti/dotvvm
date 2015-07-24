using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class EmitterMethodInfo
    {

        public List<StatementSyntax> Statements { get; set; }

        public string Name { get; set; }

        public int ControlIndex { get; set; }
        

        public EmitterMethodInfo()
        {
            Statements = new List<StatementSyntax>();
        }
    }
}