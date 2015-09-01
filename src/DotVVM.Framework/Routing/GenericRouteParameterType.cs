using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Routing
{
    public class GenericRouteParameterType : IRouteParameterType
    {
        string partRegex;
        public string GetPartRegex()
            => partRegex;

        Func<string, object> parser;
        public object ParseString(string value)
            => parser == null ? value : parser(value);

        public GenericRouteParameterType(string partRegex, Func<string, object> parser = null)
        {
            this.partRegex = partRegex;
            this.parser = parser;
        }
    }
}
