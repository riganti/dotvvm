using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Configuration
{
    public class RedwoodMarkupConfiguration
    {

        /// <summary>
        /// Gets the registered control namespaces.
        /// </summary>
        public List<RedwoodControlConfiguration> Controls { get; private set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodMarkupConfiguration"/> class.
        /// </summary>
        public RedwoodMarkupConfiguration()
        {
            Controls = new List<RedwoodControlConfiguration>()
            {
                new RedwoodControlConfiguration()
                {
                    TagPrefix = "rw",
                    Namespaces =
                    {
                        "Redwood.Framework.Controls"
                    }
                }
            };
        }
    }
}