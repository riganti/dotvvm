using DotVVM.Framework.Parser.Dothtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public abstract class ResolvedContentNode: IResolvedTreeNode
    {
        public DothtmlNode DothtmlNode { get; set; }
        public List<ResolvedControl> Content { get; set; }
        public ControlResolverMetadata Metadata { get; set; }

        public ResolvedContentNode(ControlResolverMetadata metadata, DothtmlNode node, List<ResolvedControl> content)
        {
            Metadata = metadata;
            DothtmlNode = node;
            Content = content;
        }

        public ResolvedContentNode(ControlResolverMetadata metadata, DothtmlNode node)
            : this(metadata, node, new List<ResolvedControl>())
        { }

        public abstract void Accept(IResolvedControlTreeVisitor visitor);

        public virtual void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var item in Content)
            {
                item.Accept(visitor);
            }
        }
    }
}
