#nullable enable

using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedContentNode : ResolvedTreeNode, IAbstractContentNode 
    {
        private List<ResolvedControl>? content;
        public List<ResolvedControl> Content
        {
            get
            {
                if (content == null)
                {
                    if (ResolveContentAction != null)
                    {
                        content = new List<ResolvedControl>();
                        ResolveContentAction();
                        ResolveContentAction = null;
                    }
                    else content = new List<ResolvedControl>();
                }
                return content;
            }
        }

        public Action? ResolveContentAction { get; set; }

        public ControlResolverMetadata Metadata { get; set; }

        public DataContextStack DataContextTypeStack { get; set; }
        
        IEnumerable<IAbstractControl> IAbstractContentNode.Content => Content;

        IControlResolverMetadata IAbstractContentNode.Metadata => Metadata;

        IDataContextStack IAbstractContentNode.DataContextTypeStack
        {
            get { return DataContextTypeStack; }
            set { DataContextTypeStack = (DataContextStack)value; }
        }

        public ResolvedContentNode(ControlResolverMetadata metadata, DothtmlNode? node, List<ResolvedControl>? content, DataContextStack dataContext)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            DothtmlNode = node;
            this.content = content;
            DataContextTypeStack = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public ResolvedContentNode(ControlResolverMetadata metadata, DothtmlNode? node, DataContextStack dataContext)
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
