using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Security {
    public interface IViewModelProtector {

        string Protect(string serializedData, RedwoodRequestContext context);

        string Unprotect(string protectedData, RedwoodRequestContext context);

    }
}
