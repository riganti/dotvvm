using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Security {
    public class DefaultViewModelProtector : IViewModelProtector {
        public string Protect(string serializedData, RedwoodRequestContext context) {
            // TODO
            return serializedData;
        }

        public string Unprotect(string protectedData, RedwoodRequestContext context) {
            // TODO
            return protectedData;
        }
    }
}
