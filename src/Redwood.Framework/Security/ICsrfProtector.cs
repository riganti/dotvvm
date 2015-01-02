using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Security {
    public interface ICsrfProtector {

        string GenerateToken(RedwoodRequestContext context);

        void VerifyToken(RedwoodRequestContext context, string token);

    }
}
