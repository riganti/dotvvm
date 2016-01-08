using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IAbstractTreeRoot : IAbstractContentNode
    {

        Dictionary<string, string> Directives { get; }

    }
}
