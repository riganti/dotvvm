using System;
using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
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

        public IDataContextStack DataContextTypeStack { get; set; }

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