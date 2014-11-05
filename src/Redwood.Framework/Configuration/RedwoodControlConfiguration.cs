using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Configuration
{
    public class RedwoodControlConfiguration
    {

        public string TagPrefix { get; set; }

        public List<string> Namespaces { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodControlConfiguration"/> class.
        /// </summary>
        public RedwoodControlConfiguration()
        {
            Namespaces = new List<string>();
        }

    }
}