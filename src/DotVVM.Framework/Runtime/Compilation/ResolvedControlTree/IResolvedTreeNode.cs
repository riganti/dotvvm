using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public interface IResolvedTreeNode
    {
        void Accept(IResolvedControlTreeVisitor visitor);
        void AcceptChildren(IResolvedControlTreeVisitor visitor);
    }
}
