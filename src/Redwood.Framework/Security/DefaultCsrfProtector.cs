using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Security {
    public class DefaultCsrfProtector : ICsrfProtector {
        public string GenerateToken(RedwoodRequestContext context) {
            // TODO
            return string.Empty;
        }

        public void VerifyToken(RedwoodRequestContext context, string token) {
            // TODO
        }
    }
}
