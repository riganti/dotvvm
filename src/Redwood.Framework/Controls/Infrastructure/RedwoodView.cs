using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls.Infrastructure
{
    /// <summary>
    /// Represents a top-level control in the control tree.
    /// </summary>
    public class RedwoodView : RedwoodBindableControl
    {

        /// <summary>
        /// Gets or sets the collection of directives.
        /// </summary>
        public Dictionary<string, string> Directives { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodView"/> class.
        /// </summary>
        public RedwoodView()
        {
            Directives = new Dictionary<string, string>();
            ResourceDependencies.Add("redwood");
        }

    }
}
