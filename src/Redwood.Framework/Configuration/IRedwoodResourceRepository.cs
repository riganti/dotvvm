using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Configuration
{
    public interface IRedwoodResourceRepository
    {
        ResourceBase FindResource(string name);
    }
}
