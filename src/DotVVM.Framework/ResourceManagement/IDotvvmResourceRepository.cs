using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public interface IDotvvmResourceRepository
    {
        ResourceBase FindResource(string name);
    }
}
