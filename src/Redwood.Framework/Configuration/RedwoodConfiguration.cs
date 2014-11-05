using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Routing;

namespace Redwood.Framework.Configuration
{
    public class RedwoodConfiguration
    {
        public const string RedwoodControlTagPrefix = "rw";

        /// <summary>
        /// Gets or sets the application physical path.
        /// </summary>
        public string ApplicationPhysicalPath { get; set; }

        /// <summary>
        /// Gets the settings of the markup.
        /// </summary>
        public RedwoodMarkupConfiguration Markup { get; private set; }

        /// <summary>
        /// Gets the route table.
        /// </summary>
        public RedwoodRouteTable RouteTable { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodConfiguration"/> class.
        /// </summary>
        public RedwoodConfiguration()
        {
            Markup = new RedwoodMarkupConfiguration();
            RouteTable = new RedwoodRouteTable(this);
        }
    }
}
