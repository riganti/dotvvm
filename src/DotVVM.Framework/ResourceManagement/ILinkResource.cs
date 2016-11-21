using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public interface ILinkResource: IResource
    {
        IEnumerable<IResourceLocation> GetLocations();
        string MimeType { get; }
    }
}
