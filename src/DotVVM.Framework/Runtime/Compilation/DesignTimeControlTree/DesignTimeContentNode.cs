using System;
using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.DesignTimeControlTree
{
    public abstract class DesignTimeContentNode : IAbstractContentNode
    {
        private readonly DesignTimeControlResolver resolver;


        public DothtmlNode DothtmlNode { get; }

        private List<DesignTimeControl> content;
        public IEnumerable<IAbstractControl> Content
        {
            get
            {
                EnsureContentResolved();
                return content;
            }
        }

        private IControlResolverMetadata metadata;

        public IControlResolverMetadata Metadata
        {
            get
            {
                EnsureMetadataResolved();
                return metadata;
            }
        }

        public IDataContextStack DataContextTypeStack
        {
            get { throw new NotImplementedException(); }
        }

        private void EnsureContentResolved()
        {
            if (content != null) return;
            ResolveContent();
        }

        private void ResolveContent()
        {
            throw new NotImplementedException();
        }

        private void EnsureMetadataResolved()
        {
            if (metadata != null) return;
            ResolveMetadata();
        }

        private void ResolveMetadata()
        {
            throw new NotImplementedException();
        }


        public DesignTimeContentNode(DothtmlNode node, DesignTimeControlResolver resolver)
        {
            this.resolver = resolver;
            DothtmlNode = node;
        }
    }
}