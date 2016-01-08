using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
{
    public class DesignTimeTreeRoot : DesignTimeContentNode, IAbstractTreeRoot
    {

        public DesignTimeTreeRoot(DothtmlRootNode node, DesignTimeControlResolver resolver) : base(node, resolver)
        {
            Directives = node.Directives.ToDictionary(d => d.Name, d => d.Value);
        }

        public Dictionary<string, string> Directives { get; }

    }
}