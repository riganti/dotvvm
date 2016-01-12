using System;
using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    public abstract class ResolvedContentNode : ResolvedTreeNode, IAbstractContentNode 
    {
        public DothtmlNode DothtmlNode { get; set; }

        public List<ResolvedControl> Content { get; set; }

        public ControlResolverMetadata Metadata { get; set; }

        public DataContextStack DataContextTypeStack { get; set; }
        
        IEnumerable<IAbstractControl> IAbstractContentNode.Content => Content;

        IControlResolverMetadata IAbstractContentNode.Metadata => Metadata;

        IDataContextStack IAbstractContentNode.DataContextTypeStack
        {
            get { return DataContextTypeStack; }
            set { DataContextTypeStack = (DataContextStack)value; }
        }

        public ResolvedContentNode(ControlResolverMetadata metadata, DothtmlNode node, List<ResolvedControl> content, DataContextStack dataContext)
        {
            Metadata = metadata;
            DothtmlNode = node;
            Content = content;
            DataContextTypeStack = dataContext;
        }

        public ResolvedContentNode(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext)
            : this(metadata, node, new List<ResolvedControl>(), dataContext)
        {
        }
        
        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var item in Content)
            {
                item.Accept(visitor);
            }
        }

        public void AddChild(ResolvedControl child)
        {
            Content.Add(child);
            child.Parent = this;
        }
    }
}
