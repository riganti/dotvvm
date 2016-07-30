using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedHtmlAttributeSetter : ResolvedTreeNode, IAbstractHtmlAttributeSetter
    {
        public string Name { get; set; }

        public ResolvedHtmlAttributeSetter(string name)
        {
            Name = name;
        }
    }
}
