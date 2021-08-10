#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Post-processes requested resource by a page.
    /// </summary>
    public interface IResourceProcessor
    {
        IEnumerable<NamedResource> Process(IEnumerable<NamedResource> source);
    }
}
