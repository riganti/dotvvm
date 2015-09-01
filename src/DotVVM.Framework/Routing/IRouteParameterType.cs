using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Routing
{
    public interface IRouteParameterType
    {
        string GetPartRegex();
        object ParseString(string value);
    }
}
