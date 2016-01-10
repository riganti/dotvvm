using System;
using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
{
    public abstract class DesignTimeContentNode : IAbstractContentNode
    {
        
        public DothtmlNode DothtmlNode { get; }


        private List<DesignTimeControl> content;
        public IEnumerable<IAbstractControl> Content
        {
            get
            {
                if (content == null)
                {
                    ResolveContentAction();
                }
                return content;
            }
        }

        public IControlResolverMetadata Metadata { get; private set; }

        public IDataContextStack DataContextTypeStack { get; set; }

        public Action ResolveContentAction { get; set; }
        

        public DesignTimeContentNode(DothtmlNode node, IControlResolverMetadata metadata)
        {
            DothtmlNode = node;
            Metadata = metadata;
        }



        public void AddChildControl(DesignTimeControl child)
        {
            if (content == null)
            {
                content = new List<DesignTimeControl>();
            }
            else
            {
                throw new InvalidOperationException("Cannot add children into a resolved tree node!");
            }

            content.Add(child);
        }
    }
}