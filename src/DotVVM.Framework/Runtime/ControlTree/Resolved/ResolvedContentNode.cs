using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    public abstract class ResolvedContentNode : IResolvedTreeNode, IAbstractContentNode 
    {
        public DothtmlNode DothtmlNode { get; set; }
        public List<ResolvedControl> Content { get; set; }
        public ControlResolverMetadata Metadata { get; set; }
        public DataContextStack DataContextTypeStack { get; set; }


        public ResolvedContentNode(ControlResolverMetadata metadata, DothtmlNode node, List<ResolvedControl> content, DataContextStack dataContext)
        {
            Metadata = metadata;
            DothtmlNode = node;
            Content = content;
            DataContextTypeStack = dataContext;
        }

        public ResolvedContentNode(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext)
            : this(metadata, node, new List<ResolvedControl>(), dataContext)
        { }

        public abstract void Accept(IResolvedControlTreeVisitor visitor);

        public virtual void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var item in Content)
            {
                item.Accept(visitor);
            }
        }


        IEnumerable<IAbstractControl> IAbstractContentNode.Content => Content;

        IControlResolverMetadata IAbstractContentNode.Metadata => Metadata;

        IDataContextStack IAbstractContentNode.DataContextTypeStack
        {
            get { return DataContextTypeStack; }
            set { DataContextTypeStack = (DataContextStack) value; }  
        } 
    }
}
