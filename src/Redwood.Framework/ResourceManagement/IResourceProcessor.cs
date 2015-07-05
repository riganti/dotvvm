using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.ResourceManagement
{
    public interface IResourceProcessor
    {
        IEnumerable<NamedResource> Process(IEnumerable<NamedResource> source);
    }
}
